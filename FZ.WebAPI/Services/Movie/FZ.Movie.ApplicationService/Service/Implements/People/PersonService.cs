using FZ.Constant;
using FZ.Movie.ApplicationService.Common;
using FZ.Movie.ApplicationService.Search;
using FZ.Movie.ApplicationService.Service.Abtracts;
using FZ.Movie.Domain.People;
using FZ.Movie.Dtos.Request;
using FZ.Movie.Infrastructure.Repository;
using FZ.Movie.Infrastructure.Repository.Catalog;
using FZ.Movie.Infrastructure.Repository.People;
using FZ.Shared.ApplicationService;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FZ.Movie.ApplicationService.Service.Implements.People
{
    public class PersonService : MovieServiceBase , IPersonService
    {
        private readonly IPersonRepository _personRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IPersonIndexService _personIndexService;
        private readonly IMovieIndexService _movieIndexService;
        private readonly IMoviePersonRepository _moviePersonRepository;
        public PersonService(
            IPersonRepository personRepository, 
            IUnitOfWork unitOfWork, 
            ILogger<PersonService> logger , 
            IPersonIndexService personIndexService,
            IMovieIndexService movieIndexService,
            IMoviePersonRepository moviePersonRepository,

            ICloudinaryService cloudinaryService) : base(logger)
        {
            _personRepository = personRepository;
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
            _personIndexService = personIndexService;
            _movieIndexService = movieIndexService;
            _moviePersonRepository = moviePersonRepository;

        }
        public async Task<ResponseDto<Person>> CreatePerson(CreatePersonRequest request, CancellationToken ct)
        {
            _logger.LogInformation("Creating a new person ");
            try
            {
                var avatarUrl = await _cloudinaryService.UploadImageAsync(request.avatar);

                Person newPerson = new Person
                {
                    fullName = request.fullName,
                    knownFor = request.knownFor,
                    biography = request.biography,
                    regionID = request.regionID,
                    avatar = avatarUrl,
                    birthDate = request.birthDate,
                    createdAt = DateTime.UtcNow,
                    updatedAt = DateTime.UtcNow,
                };
                await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
                {
                    await _personRepository.AddAsync(newPerson, cancellationToken);
                   
                    return newPerson;
                }, ct: ct);
           
                _logger.LogInformation("Person created successfully with ID: {PersonID}", newPerson.personID);
                return ResponseConst.Success("Person created successfully", newPerson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating person with name: {Name}" , request);
                return ResponseConst.Error<Person>(500, "An error occurred while creating the person");
            }
        }
        public async Task<ResponseDto<Person>> UpdatePerson(UpdatePersonRequest request, CancellationToken ct)
        {
            _logger.LogInformation("Updating person with ID: {PersonID}", request.personID);
            try
            {
                var existingPerson = await _personRepository.GetByIdAsync(request.personID, ct);
                if (existingPerson == null)
                {
                    _logger.LogWarning("Person with ID: {PersonID} not found", request.personID);
                    return ResponseConst.Error<Person>(404, "Person not found");
                }

                // 1. Lưu lại URL ảnh cũ để xóa sau khi cập nhật thành công
                string oldAvatarUrl = existingPerson.avatar;

                // 2. Upload ảnh mới (nếu có)
                if (request.avatar != null)
                {
                    var newAvatarUrl = await _cloudinaryService.UploadImageAsync(request.avatar);
                    existingPerson.avatar = newAvatarUrl;
                }

                // 3. Cập nhật các trường thông tin (Dùng ?? để giữ nguyên nếu null)
                existingPerson.fullName = request.fullName ?? existingPerson.fullName;
                existingPerson.knownFor = request.knownFor ?? existingPerson.knownFor;
                existingPerson.biography = request.biography ?? existingPerson.biography;

                // Lưu ý: Nếu regionID là nullable (int?) thì nên dùng ??, nếu không sẽ bị gán đè null
                // Nếu request.regionID là int thường thì logic này cần xem lại tùy nghiệp vụ
                if (request.regionID != null)
                {
                    existingPerson.regionID = request.regionID;
                }

                existingPerson.birthDate = request.birthDate ?? existingPerson.birthDate;
                existingPerson.updatedAt = DateTime.UtcNow;

                // 4. Lưu vào Database
                await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
                {
                    await _personRepository.UpdateAsync(existingPerson, cancellationToken);
                    return existingPerson;
                }, ct: ct);

                // 5. Xóa ảnh cũ trên Cloudinary (Chỉ xóa khi đã lưu DB thành công và có upload ảnh mới)
                if (request.avatar != null && !string.IsNullOrEmpty(oldAvatarUrl))
                {
                    try
                    {
                        // Truyền vào oldAvatarUrl (URL cũ) chứ không phải existingPerson.avatar (URL mới)
                        await _cloudinaryService.DeleteImageAsync(oldAvatarUrl);
                        _logger.LogInformation("Deleted old avatar: {OldAvatar}", oldAvatarUrl);
                    }
                    catch (Exception ex)
                    {
                        // Không return lỗi ở đây để tránh rollback transaction chỉ vì lỗi xóa ảnh cũ
                        _logger.LogError(ex, "Failed to delete old avatar for PersonID: {PersonID}", existingPerson.personID);
                    }
                }

                if (existingPerson.personID > 0)
                {
                    // SỬA LẠI: Chạy tuần tự thay vì song song để tránh lỗi DbContext concurrency
                    await _personIndexService.IndexByIdAsync(existingPerson.personID, ct);
                    await _movieIndexService.ReindexByPersonAsync(existingPerson.personID, ct);

                    _logger.LogInformation("Person indexed successfully in OpenSearch with ID: {PersonID}", existingPerson.personID);
                }

                _logger.LogInformation("Person updated successfully with ID: {PersonID}", existingPerson.personID);
                return ResponseConst.Success("Person updated successfully", existingPerson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating person with ID: {PersonID}", request.personID);
                return ResponseConst.Error<Person>(500, "An error occurred while updating the person");
            }
        }
        public async Task<ResponseDto<bool>> DeletePerson(int personID, CancellationToken ct)
        {
            _logger.LogInformation("Deleting person with ID: {PersonID}", personID);
            try
            {
                var existingPerson = await _personRepository.GetByIdAsync(personID, ct);
                if (existingPerson == null)
                {
                    _logger.LogWarning("Person with ID: {PersonID} not found", personID);
                    return ResponseConst.Error<bool>(404, "Person not found");
                }
                
                var associatedMoviePersons = await _moviePersonRepository.GetAllByPersonIdAsync(personID, ct);


                await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
                {
                    await _personRepository.RemoveAsync(existingPerson.personID);
                    foreach (var moviePerson in associatedMoviePersons)
                    {
                        await _moviePersonRepository.RemoveAsync(moviePerson.moviePersonID);
                    }

                    return true;
                }, ct: ct);
                _logger.LogInformation("Person deleted successfully with ID: {PersonID}", personID);
                if (!string.IsNullOrEmpty(existingPerson.avatar))
                {
                    await _cloudinaryService.DeleteImageAsync(existingPerson.avatar);
                    _logger.LogInformation("Person avatar deleted successfully for ID: {PersonID}", existingPerson.personID);
                }
                if (existingPerson.personID > 0)
                {
                    await _personIndexService.DeleteAsync(existingPerson.personID, ct);
                    await _movieIndexService.ReindexByPersonAsync(existingPerson.personID, ct);
                    _logger.LogInformation("Person removed from OpenSearch with ID: {PersonID}", existingPerson.personID);
                }
                return ResponseConst.Success("Person deleted successfully", true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting person with ID: {PersonID}", personID);
                return ResponseConst.Error<bool>(500, "An error occurred while deleting the person");
            }
        }
        public async Task<ResponseDto<Person>> GetPersonByID(int personID, CancellationToken ct)
        {
            _logger.LogInformation("Retrieving person with ID: {PersonID}", personID);
            try
            {
                var person = await _personRepository.GetByIdAsync(personID, ct);
                if (person == null)
                {
                    _logger.LogWarning("Person with ID: {PersonID} not found", personID);
                    return ResponseConst.Error<Person>(404, "Person not found");
                }
                _logger.LogInformation("Person retrieved successfully with ID: {PersonID}", personID);
                return ResponseConst.Success("Person retrieved successfully", person);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving person with ID: {PersonID}", personID);
                return ResponseConst.Error<Person>(500, "An error occurred while retrieving the person");
            }
        }
        public async Task<ResponseDto<List<Person>>> GetPeople(CancellationToken ct)
        {
            _logger.LogInformation("Retrieving all people");
            try
            {
                var people = await _personRepository.GetAllPersonAsync(ct);
                _logger.LogInformation("Successfully retrieved {Count} people", people.Count);
                return ResponseConst.Success("People retrieved successfully", people);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving people");
                return ResponseConst.Error<List<Person>>(500, "An error occurred while retrieving people");
            }
        }

    }
}
