using System.Data.Entity;

namespace Mapgenix.FeatureSource
{
    public class SourceContext : DbContext
    {
        public SourceContext(string name)
            : base(nameOrConnectionString: name)
        {
            var ensureDLLIsCopied = System.Data.Entity.SqlServer.SqlProviderServices.Instance;
        }
    }
}
