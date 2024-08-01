global using num = System.Numerics;
global using bint = System.Numerics.BigInteger;

#if F16
global using __scalar = System.Half;
#elif F32
global using __scalar = float;
#elif F64
global using __scalar = double;
#elif D128
global using __scalar = decimal;
#else
global using __scalar = double;
#endif


using System;


[assembly: CLSCompliant(false)]
