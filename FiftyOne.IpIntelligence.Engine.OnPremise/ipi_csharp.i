%include "./ip-intelligence-cxx/src/common-cxx/CsTypes.i"

%include "./ip-intelligence-cxx/src/ipi.i"

%typemap(cstype) char* "string"
%typemap(imtype) char* "System.IntPtr"
%typemap(csin) char* "UTF8Marshaler.GetInstance(null).MarshalManagedToNative($csinput)"
%typemap(csout) char* {
    return (string)UTF8Marshaler.GetInstance(null).MarshalNativeToManaged($imcall);
}
