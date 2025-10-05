using FZ.Auth.ApplicationService.Common;
using FZ.Auth.Domain.Billing;
using FZ.Auth.Dtos.Billing;
using FZ.Auth.Infrastructure.Repository.Abtracts;
using FZ.Auth.Infrastructure.Repository.Billing;
using FZ.Constant;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Auth.ApplicationService.Billing
{
    public interface IPlanService
    {
        Task<ResponseDto<Plan>> CreatePlan(CreatePlanRequestDto plan , CancellationToken ct = default);
        Task<ResponseDto<Plan>> UpdatePlan(UpdatePlanRequestDto plan , CancellationToken ct = default);
        Task<ResponseDto<Plan>> DeletePlan(int planID , CancellationToken ct = default);
        Task<ResponseDto<Plan>> GetPlanByID(int planID, CancellationToken ct = default);
        Task<ResponseDto<List<Plan>>> GetAllPlans();
    }
    public class PlanService : AuthServiceBase, IPlanService
    {
        private readonly IUnitOfWork _uow;
        private readonly IPlanRepository _planRepo;

        public PlanService(IUnitOfWork uow, IPlanRepository planRepository, ILogger<PlanService> logger) : base(logger)
        {
            _uow = uow;
            _planRepo = planRepository;
        }
        public async Task<ResponseDto<Plan>> CreatePlan(CreatePlanRequestDto plan, CancellationToken ct = default)
        {
            _logger.LogInformation("Creating new plan with code: {PlanCode}", plan.code);
            try
            {
                var createplan = new Plan
                {
                    code = plan.code,
                    name = plan.name,
                    description = plan.description,
                    isActive = plan.isActive
                };

                var createdPlan = await _uow.ExecuteInTransactionAsync(async t =>
                {
                    var newPlan = await _planRepo.AddAsync(createplan, t);
                    // ❌ KHÔNG SaveChanges ở đây
                    return newPlan;
                }, ct: ct);

                _logger.LogInformation("Successfully created plan with ID: {PlanID}", createdPlan.planID);
                return ResponseConst.Success("Tạo plan thành công", createdPlan);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating plan with code: {PlanCode}", plan.code);
                return ResponseConst.Error<Plan>(500, "Tạo plan thất bại"); // đừng để rơi khỏi hàm mà không return
            }
        }
        public async Task<ResponseDto<Plan>> UpdatePlan(UpdatePlanRequestDto plan, CancellationToken ct = default)
        {
            _logger.LogInformation("Updating plan with ID: {PlanID}", plan.planID);
            try
            {
                var existingPlan = await _planRepo.GetByIDAsync(plan.planID, ct);
                if (existingPlan == null)
                {
                    _logger.LogWarning("Plan with ID: {PlanID} not found", plan.planID);
                    return ResponseConst.Error<Plan>(404, "Plan không tồn tại");
                }

                var plans = new Plan
                {
                    planID = plan.planID,
                    code = plan.code,
                    name = plan.name,
                    description = plan.description,
                    isActive = plan.isActive
                };

                var updatedPlan = await _uow.ExecuteInTransactionAsync(async t =>
                {
                    

                    var updated = await _planRepo.UpdateAsync(plans, t);
                    return updated;
                }, ct: ct);
                if (updatedPlan == null)
                {
                    return ResponseConst.Error<Plan>(404, "Plan không tồn tại");
                }
                _logger.LogInformation("Successfully updated plan with ID: {PlanID}", updatedPlan.planID);
                return ResponseConst.Success("Cập nhật plan thành công", updatedPlan);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating plan with ID: {PlanID}", plan.planID);
                return ResponseConst.Error<Plan>(500, "Cập nhật plan thất bại");
            }
        }
        public async Task<ResponseDto<Plan>> DeletePlan(int planID , CancellationToken ct = default)
        {
            _logger.LogInformation("Deleting plan with ID: {PlanID}", planID);
            try
            {
                var plan = await _planRepo.GetByIDAsync(planID, ct);
                if (plan == null)
                {
                    _logger.LogWarning("Plan with ID: {PlanID} not found", planID);
                    return ResponseConst.Error<Plan>(404, "Plan không tồn tại");
                }

                var deletedPlan = await _uow.ExecuteInTransactionAsync(async t =>
                {
                    var deleted = await _planRepo.DeleteAsync(plan, t);
                    return deleted;
                }, ct: ct);
                if (deletedPlan == null)
                {
                    return ResponseConst.Error<Plan>(404, "Plan không tồn tại");
                }
                _logger.LogInformation("Successfully deleted plan with ID: {PlanID}", deletedPlan.planID);
                return ResponseConst.Success("Xoá plan thành công", deletedPlan);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting plan with ID: {PlanID}", planID);
                return ResponseConst.Error<Plan>(500, "Xoá plan thất bại");
            }
        }
        public async Task<ResponseDto<Plan>> GetPlanByID(int planID, CancellationToken ct = default)
        {
            _logger.LogInformation("Retrieving plan with ID: {PlanID}", planID);
            try
            {
                var plan = await _planRepo.GetByIDAsync(planID, ct);
                if (plan == null)
                {
                    _logger.LogWarning("Plan with ID: {PlanID} not found", planID);
                    return ResponseConst.Error<Plan>(404, "Plan không tồn tại");
                }
                _logger.LogInformation("Successfully retrieved plan with ID: {PlanID}", plan.planID);
                return ResponseConst.Success("Lấy plan thành công", plan);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving plan with ID: {PlanID}", planID);
                return ResponseConst.Error<Plan>(500, "Lấy plan thất bại");
            }
        }
        public async Task<ResponseDto<List<Plan>>> GetAllPlans()
        {
            _logger.LogInformation("Retrieving all plans");
            try
            {
                var plans = await _planRepo.GetAllAsync(CancellationToken.None);
                _logger.LogInformation("Successfully retrieved {PlanCount} plans", plans.Count);
                return ResponseConst.Success("Lấy danh sách plan thành công", plans);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all plans");
                return ResponseConst.Error<List<Plan>>(500, "Lấy danh sách plan thất bại");
            }

        }
       
    }
}
       
