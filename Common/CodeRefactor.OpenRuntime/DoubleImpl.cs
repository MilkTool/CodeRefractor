using CodeRefractor.Runtime.Annotations;

namespace CodeRefactor.OpenRuntime
{
    [ExtensionsImplementation(typeof(double))]
    public static class DoubleImpl
    {
        [MapMethod(IsStatic = true)]
        [CppMethodBody(
            Header = "cwchar",
            Code = @" 
	            return wcstod(text->Text->Items,NULL);"
        )]
        public static double Parse(string text)
        {
            return 0;
        }
    }
}