using Services.Interfaces;

namespace Services.FacadeService
{
    public interface IFacadeService
    {
        IOrderService OrderService { get; }
        IProductService ProductService { get; }
        IUserService UserService { get; }
        ICategoryService CategoryService { get; }
        IPaymentService PaymentService { get; }
    }
}
