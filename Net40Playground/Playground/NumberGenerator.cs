using Spackle;
using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace Playground
{
	public static class NumberGenerator
	{
		private const string ErrorInvalidNumberOfDigits = "The number of digits must be greater than 0.";

		public static BigInteger CreateBigInteger(uint numberOfDigits)
		{
			if(numberOfDigits == 0)
			{
				throw new ArgumentException(NumberGenerator.ErrorInvalidNumberOfDigits, 
					"numberOfDigits");
			}

			var digit = BigInteger.Zero;

			using(var random = new SecureRandom())
			{
				if(numberOfDigits == 1)
				{
					digit = new BigInteger(random.Next(0, 10));
				}
				else
				{
					var remainingDigits = numberOfDigits;

					while(remainingDigits > 0)
					{
						if(remainingDigits >= 9)
						{
							digit = (1000000000 * digit) + random.Next(100000000, 1000000000);
							remainingDigits -= 9;
						}
						else
						{
							digit = ((int)(Math.Pow(10, remainingDigits)) * digit) +
								random.Next((int)(Math.Pow(10, remainingDigits - 1)), 
									(int)(Math.Pow(10, remainingDigits)));
							remainingDigits = 0;
						}
					}
				}

				return digit;
			}
		}

		public static Complex CreateComplex()
		{
			using(var random = new SecureRandom())
			{
				return new Complex(random.NextDouble() * 100.0,
					random.NextDouble() * 100.0);
			}
		}

		public static Tuple<BigInteger, DateTime, Guid>
			CreateBigIntegerWithLotsOfInformation(uint numberOfDigits)
		{
			return new Tuple<BigInteger, DateTime, Guid>(
				NumberGenerator.CreateBigInteger(numberOfDigits),
				DateTime.UtcNow, Guid.NewGuid());
		}

		public static int GetLength(string value)
		{
			if(value == null)
			{
				throw new ArgumentNullException("value");
			}

			return value.Length;
		}

		public static Tuple<BigInteger, DateTime, Guid> 
			CreateContractedBigIntegerWithLotsOfInformation(uint numberOfDigits)
		{
			Contract.Requires<ArgumentException>(
				numberOfDigits > 0, NumberGenerator.ErrorInvalidNumberOfDigits);
			Contract.Ensures(
				Contract.Result<Tuple<BigInteger, DateTime, Guid>>() != null);

			return NumberGenerator.CreateBigIntegerWithLotsOfInformation(numberOfDigits);
		}

		public static BigInteger CreateBigInteger(uint numberOfDigits, CreationOptions options)
		{
			var result = NumberGenerator.CreateBigInteger(numberOfDigits);

			if(options == CreationOptions.CreateOdd && result.IsEven)
			{
				result--;
			}
			else if(options == CreationOptions.CreateEven && !result.IsEven)
			{
				result++;
			}

			return result;
		}
	}
}
