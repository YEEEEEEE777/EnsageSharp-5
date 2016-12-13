using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxeSharpNew
{
    internal class Program
    {
        #region Static Fields

        private static readonly Bootstrap BootstrapInstance = new Bootstrap();

        #endregion

        #region Methods

        private static void Main()
        {
            BootstrapInstance.Initialize();
        }

        #endregion
    }
}
