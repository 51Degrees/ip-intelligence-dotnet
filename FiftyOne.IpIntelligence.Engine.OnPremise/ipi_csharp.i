%include "./ip-intelligence-cxx/src/common-cxx/CsTypes.i"

%include "./ip-intelligence-cxx/src/ipi.i"

// %typemap(cstype) char* "string"
// %typemap(imtype) char* "System.IntPtr"
// %typemap(csin) char* "UTF8Marshaler.GetInstance(null).MarshalManagedToNative($csinput)"
// %typemap(csout) char* {
//     return (string)UTF8Marshaler.GetInstance(null).MarshalNativeToManaged($imcall);
// }

// %include <std_string.i>

////////////////////////////////////////////////////////////////////////
// 1) Tell SWIG we want C# strings for std::string
////////////////////////////////////////////////////////////////////////
%typemap(cstype)    std::string "string"
%typemap(imtype)    std::string "System.IntPtr"

////////////////////////////////////////////////////////////////////////
// 2) C# → C++  (csin = “C# side input”)
////////////////////////////////////////////////////////////////////////
%typemap(csin) std::string %{
    // build native UTF-8 buffer
    System.IntPtr __utf8 = UTF8Marshaler.GetInstance(null)
                           .MarshalManagedToNative($csinput);
    $csinput = __utf8;           // pass IntPtr into P/Invoke stub
%}

// In the _generated_ C wrapper, turn that IntPtr into a std::string
%typemap(in) std::string %{
    // $input is a `char*` pointing at a UTF-8 buffer
    $1 = std::string((char*) $input);
    // free the buffer your marshaler gave us
    CoTaskMemFree($input);
%}

////////////////////////////////////////////////////////////////////////
// 3) C++ → C#  (csout = “C# side output”)
////////////////////////////////////////////////////////////////////////
// In the C wrapper, extract a char* via c_str() and hand it back
%typemap(out) std::string %{
    const char* __s = $1.c_str();
    // copy into a new buffer so it survives after $1 is destroyed
    char* __copy = (char*) malloc(strlen(__s) + 1);
    strcpy(__copy, __s);
    $result = __copy;
%}

// Finally, in the C# stub, marshal that char* → string
%typemap(csout) std::string %{
    // $imcall is an IntPtr to our malloc’ed UTF-8 buffer
    string __ret = (string) UTF8Marshaler
                       .GetInstance(null)
                       .MarshalNativeToManaged($imcall);
    return __ret;
%}
