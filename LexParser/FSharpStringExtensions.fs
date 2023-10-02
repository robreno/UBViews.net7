[<AutoOpen>]
module Microsoft.FSharp.Core.String.FSharpStringExtensions

open System.Runtime.CompilerServices
open System.Security.Cryptography
open System.Text

// https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/type-extensions

// https://stackoverflow.com/questions/36845430/persistent-hashcode-for-strings
//public static class StringExtensionMethods
//{
//    public static int GetStableHashCode(this string str)
//    {
//        unchecked
//        {
//            int hash1 = 5381;
//            int hash2 = hash1;

//            for(int i = 0; i < str.Length && str[i] != '\0'; i += 2)
//            {
//                hash1 = ((hash1 << 5) + hash1) ^ str[i];
//                if (i == str.Length - 1 || str[i+1] == '\0')
//                    break;
//                hash2 = ((hash2 << 5) + hash2) ^ str[i+1];
//            }

//            return hash1 + (hash2*1566083941);
//        }
//    }
//}

//static string GetSha256Hash(SHA256 shaHash, string input)
//{
//    // Convert the input string to a byte array and compute the hash.
//    byte[] data = shaHash.ComputeHash(Encoding.UTF8.GetBytes(input));

//    // Create a new Stringbuilder to collect the bytes
//    // and create a string.
//    StringBuilder sBuilder = new StringBuilder();

//    // Loop through each byte of the hashed data 
//    // and format each one as a hexadecimal string.
//    for (int i = 0; i < data.Length; i++)
//    {
//        sBuilder.Append(data[i].ToString("x2"));
//    }

//    // Return the hexadecimal string.
//    return sBuilder.ToString();
//}

type System.String with
    member x.Right(index) = x.Substring(x.Length - index)

// https://stackoverflow.com/questions/384442/f-byte-hex-string-conversion

[<Extension>]
type StringExtensions =
    
    [<Extension>] static member GetStableHashCode(source:string) =
                    use md5 = MD5.Create()
                    md5.Initialize()
                    md5.ComputeHash(Encoding.UTF8.GetBytes(source)) |> ignore
                    let hash = md5.Hash
                    hash
                    |> Array.fold (fun state x-> state + sprintf "%02X" x) ""

    [<Extension>] static member GetSha256Hash(source:string) =
                    use sha256 = SHA256.Create()
                    sha256.Initialize()
                    sha256.ComputeHash(Encoding.UTF8.GetBytes(source)) |> ignore
                    let hash = sha256.Hash
                    hash
                    |> Array.fold (fun state x-> state + sprintf "%02X" x) ""
