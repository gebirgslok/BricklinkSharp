using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace BricklinkSharp.Client
{
    public interface IBricklinkRequestHandler
    {
        Task OnRequestAsync(CancellationToken ct);
    }
}
