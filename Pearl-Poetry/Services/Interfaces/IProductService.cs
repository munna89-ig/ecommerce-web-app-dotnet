using Pearl_Poetry.ViewModels.Admin.Product;

namespace Pearl_Poetry.Services.Interfaces
{
    public interface IProductService
    {
        Task<ProductListViewModel> GetProductsAsync(
            int page = 1,
            int pageSize = 10);

        Task<ProductDetailsViewModel?> GetDetailsAsync(int id);

        Task<ProductEditViewModel?> GetEditAsync(int id);

        Task<bool> UpdateAsync(ProductEditViewModel model);

        Task<bool> DeleteAsync(int id);

        Task<bool> CreateAsync(
            ProductCreateViewModel model);
    }
}