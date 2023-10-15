using System.Threading;
using System.Threading.Tasks;

ThreadLocal<GlobalDecompileContext> DECOMPILE_CONTEXT = new ThreadLocal<GlobalDecompileContext>(() => new GlobalDecompileContext(Data, false));