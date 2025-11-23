namespace ManualMate.Application.Exceptions
{
    public class NotFoundException : ApplicationException
    {
        public NotFoundException(string name, object id)
            : base($"{name} with Id :({id}) doesn't exist")
        {
            
        }
    }
}
