using AutoMapper;
using DataAccess.DTOs.UserDTOs;
using Repositories.UnitOfWork;
using Services.Interfaces;

namespace Services.Implements
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<UserDto?> LoginAsync(LoginDTO loginDto)
        {
            var user = await _uow.UserRepository.GetByEmailAndPasswordAsync(
                loginDto.Email,
                loginDto.Password
            );

            return user == null ? null : _mapper.Map<UserDto>(user);
        }
    }
}
