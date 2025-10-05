using FZ.Auth.ApplicationService.Common;
using FZ.Auth.Domain.Billing;
using FZ.Auth.Dtos.Billing;
using FZ.Auth.Infrastructure.Repository.Abtracts;
using FZ.Auth.Infrastructure.Repository.Billing;
using FZ.Constant;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Auth.ApplicationService.Billing
{
    public interface IPriceService
    {
        Task<ResponseDto<Price>> CreatePrice(CreatePriceRequestDto price, CancellationToken ct = default);
        Task<ResponseDto<Price>> UpdatePrice(UpdatePriceRequestDto price, CancellationToken ct = default);
        Task<ResponseDto<Price>> DeletePrice(int priceID, CancellationToken ct = default);
        Task<ResponseDto<Price>> GetPrice(int priceID, CancellationToken ct = default);
        Task<ResponseDto<List<Price>>> GetAllPrices(CancellationToken ct);
    }
    public class PriceService : AuthServiceBase, IPriceService
    {
        private readonly IUnitOfWork _uow;
        private readonly IPriceRepository _priceRepo;
        public PriceService(IUnitOfWork uow, IPriceRepository priceRepository, ILogger<PriceService> logger) : base(logger)
        {
            _uow = uow;
            _priceRepo = priceRepository;
        }
        public async Task<ResponseDto<Price>> CreatePrice(CreatePriceRequestDto price, CancellationToken ct = default)
        {
            _logger.LogInformation("Creating new price with code: {PriceCode}", price.planID);
            try
            {
                var createprice = new Price
                {
                    currency = price.currency,
                    amount = price.amount,
                    intervalUnit = price.intervalUnit,
                    intervalCount = price.intervalCount,
                    trialDays = price.trialDays,
                    isActive = price.isActive,
                    planID = price.planID
                };
                var createdPrice = await _uow.ExecuteInTransactionAsync(async t =>
                {
                    var newPrice = await _priceRepo.AddAsync(createprice, t);
                    // ❌ KHÔNG SaveChanges ở đây
                    return newPrice;
                }, ct: ct);
                _logger.LogInformation("Successfully created price with ID: {PriceID}", createdPrice.priceID);
                return ResponseConst.Success("Tạo price thành công", createdPrice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating price with code: {PriceCode}", price.planID);
                return ResponseConst.Error<Price>(500, "Lỗi khi tạo price");
            }
        }
        public async Task<ResponseDto<Price>> UpdatePrice(UpdatePriceRequestDto price, CancellationToken ct = default)
        {
            _logger.LogInformation("Updating price with ID: {PriceID}", price.priceID);
            try
            {
                var existingPrice = await _priceRepo.GetByIdAsync(price.priceID, ct);
                if (existingPrice == null)
                {
                    _logger.LogWarning("Price with ID: {PriceID} not found", price.priceID);
                    return ResponseConst.Error<Price>(404, "Price không tồn tại");
                }
                existingPrice.currency = price.currency;
                existingPrice.amount = price.amount;
                existingPrice.intervalUnit = price.intervalUnit;
                existingPrice.intervalCount = price.intervalCount;
                existingPrice.trialDays = price.trialDays;
                existingPrice.isActive = price.isActive;
                existingPrice.planID = price.planID;
                var updatedPrice = await _uow.ExecuteInTransactionAsync(async t =>
                {
                    var updPrice = await _priceRepo.UpdateAsync(existingPrice, t);
                    // ❌ KHÔNG SaveChanges ở đây
                    return updPrice;
                }, ct: ct);
                _logger.LogInformation("Successfully updated price with ID: {PriceID}", updatedPrice.priceID);
                return ResponseConst.Success("Cập nhật price thành công", updatedPrice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating price with ID: {PriceID}", price.priceID);
                return ResponseConst.Error<Price>(500, "Lỗi khi cập nhật price");
            }
        }
        public async Task<ResponseDto<Price>> DeletePrice(int priceID, CancellationToken ct = default)
        {
            _logger.LogInformation("Deleting price with ID: {PriceID}", priceID);
            try
            {
                var existingPrice = await _priceRepo.GetByIdAsync(priceID, ct);
                if (existingPrice == null)
                {
                    _logger.LogWarning("Price with ID: {PriceID} not found", priceID);
                    return ResponseConst.Error<Price>(404, "Price không tồn tại");
                }
                var deletedPrice = await _uow.ExecuteInTransactionAsync(async t =>
                {
                    var delPrice = await _priceRepo.DeleteAsync(existingPrice, t);
                    // ❌ KHÔNG SaveChanges ở đây
                    return delPrice;
                }, ct: ct);
                _logger.LogInformation("Successfully deleted price with ID: {PriceID}", deletedPrice.priceID);
                return ResponseConst.Success("Xoá price thành công", deletedPrice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting price with ID: {PriceID}", priceID);
                return ResponseConst.Error<Price>(500, "Lỗi khi xoá price");
            }
        }
        public async Task<ResponseDto<Price>> GetPrice(int priceID, CancellationToken ct = default)
        {
            _logger.LogInformation("Retrieving price with ID: {PriceID}", priceID);
            try
            {
                var price = await _priceRepo.GetByIdAsync(priceID, ct);
                if (price == null)
                {
                    _logger.LogWarning("Price with ID: {PriceID} not found", priceID);
                    return ResponseConst.Error<Price>(404, "Price không tồn tại");
                }
                _logger.LogInformation("Successfully retrieved price with ID: {PriceID}", price.priceID);
                return ResponseConst.Success("Lấy thông tin price thành công", price);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving price with ID: {PriceID}", priceID);
                return ResponseConst.Error<Price>(500, "Lỗi khi lấy thông tin price");
            }
        }
        public async Task<ResponseDto<List<Price>>> GetAllPrices(CancellationToken ct)
        {
            _logger.LogInformation("Retrieving all prices");
            try
            {
                var prices = await _priceRepo.GetAllAsync(ct);
                _logger.LogInformation("Successfully retrieved {Count} prices", prices.Count);
                return ResponseConst.Success("Lấy danh sách price thành công", prices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all prices");
                return ResponseConst.Error<List<Price>>(500, "Lỗi khi lấy danh sách price");
            }
        }
    }
}

       
