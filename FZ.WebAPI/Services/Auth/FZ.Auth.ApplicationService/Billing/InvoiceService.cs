using FZ.Auth.Domain.Billing;
using FZ.Auth.Infrastructure.Repository.Billing;
using FZ.Constant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Auth.ApplicationService.Billing
{
    public interface IInvoiceService
    {
        Task<ResponseDto<Invoice>> GetInvoiceByOrderID(int orderID, CancellationToken ct);
        Task<ResponseDto<List<Invoice>>> GetAllInvoices();
        Task<ResponseDto<List<Invoice>>> GetByUserID (int userID, CancellationToken ct);
    }
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepo;
        public InvoiceService (IInvoiceRepository invoiceRepo)
        {
            _invoiceRepo = invoiceRepo;
        }
        public async Task<ResponseDto<Invoice>> GetInvoiceByOrderID(int orderID, CancellationToken ct)
        {
            try
            {
                var invoice = await _invoiceRepo.GetByIdAsync(orderID, ct);
                if (invoice == null)
                {
                    return ResponseConst.Error<Invoice>(500, "Invoice not found");
                }
                return ResponseConst.Success("Invoice retrieved successfully", invoice);
            }
            catch (Exception ex)
            {
                return ResponseConst.Error<Invoice>(500, $"An error occurred: {ex.Message}");
            }
           
        }

        public async Task<ResponseDto<List<Invoice>>> GetAllInvoices()
        {
            try
            {
                var invoices = await _invoiceRepo.GetAllAsync(CancellationToken.None);
                return ResponseConst.Success("Invoices retrieved successfully", invoices);
            }
            catch (Exception ex)
            {
                return ResponseConst.Error<List<Invoice>>(500, $"An error occurred: {ex.Message}");
            }
        }
        public async Task<ResponseDto<List<Invoice>>> GetByUserID(int userID, CancellationToken ct)
        {
            try
            {
                var invoices = await _invoiceRepo.GetInvoicesByUserID(userID, ct);
                return ResponseConst.Success("Invoices retrieved successfully", invoices);

            }
            catch (Exception ex)
            {
                return ResponseConst.Error<List<Invoice>>(500, $"An error occurred: {ex.Message}");
            }

        }
    }
}
