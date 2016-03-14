using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeReviewApp
{
    class Testing
    {
        static void Main(string[] args)
        {
            JobLogger logger = new JobLogger(true, true, true, true, true, true);
            logger.LogMessage(Enums.LogType.Error, "this is an error");
        }
    }
}
