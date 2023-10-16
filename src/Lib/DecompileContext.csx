using System.Threading;
using System.Threading.Tasks;

// need for decompiling files
ThreadLocal<GlobalDecompileContext> DECOMPILE_CONTEXT = new ThreadLocal<GlobalDecompileContext>(() => new GlobalDecompileContext(Data, false));

/// <summary>
/// Decompile Undertale code
/// </summary>
/// <param name="code"></param>
/// <returns></returns>
string Decompile (UndertaleCode code)
{
    return Decompiler.Decompile(code, DECOMPILE_CONTEXT.Value);
}