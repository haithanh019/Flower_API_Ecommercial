using AutoMapper;
using Repositories.UnitOfWork;
using Services.Implements;
using Services.Interfaces;

namespace Services.FacadeService
{
    public class FacadeService : IFacadeService
    {
        public IOrderService OrderService { get; }
        public IProductService ProductService { get; }
        public IUserService UserService { get; }
        public ICategoryService CategoryService { get; }
        public IPaymentService PaymentService { get; }

        public FacadeService(IUnitOfWork uow, IMapper mapper)
        {
            OrderService = new OrderService(uow, mapper);
            ProductService = new ProductService(uow, mapper);
            UserService = new UserService(uow, mapper);
            CategoryService = new CategoryService(uow, mapper);
            PaymentService = new PaymentService(uow, mapper);
        }
    }
}
