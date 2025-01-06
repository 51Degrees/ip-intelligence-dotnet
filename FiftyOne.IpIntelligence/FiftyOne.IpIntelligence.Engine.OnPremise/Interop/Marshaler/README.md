# UTF 8 Marshaler

This class UTF8Marshaler is a work of [David Jeske](https://www.codeproject.com/Articles/138614/Advanced-Topics-in-PInvoke-String-Marshaling) and is distributed under the [Code Project Open License (CPOL) 1.02](https://www.codeproject.com/info/cpol10.aspx).

This README lists the changes that have been made to the original source code, complying with Terms and Conditions of the license.

## Changes made

- Added warning suppression for unused parameter 'cookie' in function ``GetInstance``

- Replaced the exception string in ``MarshalDirectiveException`` with   code to fetch the string from resource file.