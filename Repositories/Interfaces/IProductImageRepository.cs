using Business.Entities;

namespace Repositories.Interfaces
{
    public interface IProductImageRepository : IRepository<ProductImage>
    {
        Task<List<ProductImage>> GetByProductIdAsync(int productId);
    }
}
