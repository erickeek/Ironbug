using System.Data.Entity.ModelConfiguration.Conventions;

namespace IronBug.Context.Conventions
{
    public class MakeAllStringsNonUnicode : Convention
    {
        public MakeAllStringsNonUnicode()
        {
            Properties<string>().Configure(c => c.IsUnicode(false));
        }
    }
}