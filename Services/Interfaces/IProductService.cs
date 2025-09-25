using DataAccess.DTOs.ProductDTOs;

namespace Services.Interfaces;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllAsync();
    Task<ProductDto?> GetByIdAsync(int productId);
    Task<int> CreateAsync(ProductCreateDto dto);
    Task<bool> UpdateAsync(int productId, ProductUpdateDto dto);
    Task<bool> DeleteAsync(int productId);
}
