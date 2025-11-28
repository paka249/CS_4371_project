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
		
		// BFV (currently active - integer arithmetic)
		EncryptionParams = new EncryptionParameters(SchemeType.BFV);
		const ulong polyModulusDegree = 2048;
		EncryptionParams.PolyModulusDegree = polyModulusDegree;
		EncryptionParams.CoeffModulus = CoeffModulus.BFVDefault(polyModulusDegree);
		EncryptionParams.PlainModulus = new Modulus(1024);
		Context = new SEALContext(EncryptionParams);
		
		/*
		 CKKS (floating-point arithmetic - uncomment to use)
		 EncryptionParams = new EncryptionParameters(SchemeType.CKKS);
		 const ulong polyModulusDegree = 8192;
		 EncryptionParams.PolyModulusDegree = polyModulusDegree;
		 EncryptionParams.CoeffModulus = CoeffModulus.Create(polyModulusDegree, new int[] { 60, 40, 40, 60 });
		 Context = new SEALContext(EncryptionParams);
		*/
	}	}
}
