using AutoMapper;
using AutoMapper.QueryableExtensions;
using Business.Entities;
using DataAccess.DTOs.ProductDTOs;
using Microsoft.EntityFrameworkCore;
using Repositories.UnitOfWork;
using Services.Interfaces;

namespace Services.Implements
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public ProductService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync() =>
            await _uow
                .ProductRepository.Query()
                .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

        public async Task<ProductDto?> GetByIdAsync(int productId) =>
            await _uow
                .ProductRepository.Query()
                .Where(p => p.ProductId == productId)
                .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();

        public async Task<int> CreateAsync(ProductCreateDto dto)
        {
            var entity = _mapper.Map<Product>(dto);
            await _uow.ProductRepository.AddAsync(entity);

            if (dto.ImageUrls != null && dto.ImageUrls.Count > 0)
            {
                foreach (
                    var url in dto.ImageUrls.Where(u => !string.IsNullOrWhiteSpace(u)).Distinct()
                )
                {
                    await _uow.ProductImageRepository.AddAsync(
                        new ProductImage { ProductId = entity.ProductId, ImageUrl = url.Trim() }
                    );
                }
            }
            return entity.ProductId;
        }

        public async Task<bool> UpdateAsync(int productId, ProductUpdateDto dto)
        {
            var entity = await _uow.ProductRepository.GetByIdAsync(productId);
            if (entity == null)
                return false;

            _mapper.Map(dto, entity);
            entity.UpdatedAt = DateTime.UtcNow;
            await _uow.ProductRepository.UpdateAsync(entity);

            // Reconcile ảnh
            var existing = await _uow.ProductImageRepository.GetByProductIdAsync(productId);
            var desired = new HashSet<string>(
                (dto.ImageUrls ?? new())
                    .Where(u => !string.IsNullOrWhiteSpace(u))
                    .Select(u => u.Trim())
            );

            // Xóa ảnh không còn trong danh sách mong muốn
            foreach (var img in existing.Where(e => !desired.Contains(e.ImageUrl)))
            {
                await _uow.ProductImageRepository.DeleteAsync(img);
            }

            // Thêm ảnh mới
            var existingUrls = existing.Select(e => e.ImageUrl).ToHashSet();
            foreach (var url in desired.Except(existingUrls))
            {
                await _uow.ProductImageRepository.AddAsync(
                    new ProductImage { ProductId = productId, ImageUrl = url }
                );
            }

            return true;
        }

        public async Task<bool> DeleteAsync(int productId)
        {
            var entity = await _uow.ProductRepository.GetByIdAsync(productId);
            if (entity == null)
                return false;
            await _uow.ProductRepository.DeleteAsync(entity);
            return true;
        }
    }
}
