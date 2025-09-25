using AutoMapper;
using AutoMapper.QueryableExtensions;
using Business.Entities;
using DataAccess.DTOs.CategoryDTOs;
using Microsoft.EntityFrameworkCore;
using Repositories.UnitOfWork;
using Services.Interfaces;

namespace Services.Implements
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public CategoryService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllAsync() =>
            await _uow
                .CategoryRepository.Query()
                .ProjectTo<CategoryDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

        public async Task<CategoryDto?> GetByIdAsync(int categoryId)
        {
            var entity = await _uow.CategoryRepository.GetByIdAsync(categoryId);
            return entity == null ? null : _mapper.Map<CategoryDto>(entity);
        }

        public async Task<int> CreateAsync(CategoryCreateDto dto)
        {
            var entity = _mapper.Map<Category>(dto);
            await _uow.CategoryRepository.AddAsync(entity);
            return entity.CategoryId;
        }

        public async Task<bool> UpdateAsync(int categoryId, CategoryUpdateDto dto)
        {
            var entity = await _uow.CategoryRepository.GetByIdAsync(categoryId);
            if (entity == null)
                return false;

            _mapper.Map(dto, entity);
            entity.UpdatedAt = DateTime.UtcNow;
            await _uow.CategoryRepository.UpdateAsync(entity);
            return true;
        }

        public async Task<bool> DeleteAsync(int categoryId)
        {
            var entity = await _uow.CategoryRepository.GetByIdAsync(categoryId);
            if (entity == null)
                return false;

            await _uow.CategoryRepository.DeleteAsync(entity);
            return true;
        }
    }
}
