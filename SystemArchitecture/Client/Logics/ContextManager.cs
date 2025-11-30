using System;
using Microsoft.Research.SEAL;

namespace CDTS_PROJECT.Logics
{
	public class ContextManager
	{
	
		public EncryptionParameters EncryptionParams { get; set; }
		public SEALContext Context { get; set; }
		
	public ContextManager()
	{
		// CHOOSE ENCRYPTION SCHEME 
		// Comment/Uncomment one section below:
		
		// BFV (currently active )
		/*
		EncryptionParams = new EncryptionParameters(SchemeType.BFV);
		const ulong polyModulusDegree = 2048;
		EncryptionParams.PolyModulusDegree = polyModulusDegree;
		EncryptionParams.CoeffModulus = CoeffModulus.BFVDefault(polyModulusDegree);
		EncryptionParams.PlainModulus = new Modulus(1024);
		Context = new SEALContext(EncryptionParams);
		*/
		
		 //CKKS //(floating-point arithmetic )
		 
		 EncryptionParams = new EncryptionParameters(SchemeType.CKKS);
		 const ulong polyModulusDegree = 4096;
		 EncryptionParams.PolyModulusDegree = polyModulusDegree;
		 EncryptionParams.CoeffModulus = CoeffModulus.BFVDefault(polyModulusDegree);
		 Context = new SEALContext(EncryptionParams);
		
	}	}
}
