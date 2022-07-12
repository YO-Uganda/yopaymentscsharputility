# Yo Payments Utility Library for C# Example

Yo Payments is a revolutionary mobile payments gateway service. Yo! Payments enables businesses to receive payments from their customers via mobile money, as well as make mobile money payments to any mobile money account holder. Yo! Payments also has the capability to send mobile calling credit (“airtime”) directly to users. 

This utility program enables you to integrate and invoke some of the commonest Yo Payments API methods described in the official Yo Payments documentation. Use the examples written in the Program class (which is the main class), replacing the variables in there (such the apiusername, apipassword, etc) with yours.

## Getting Started
This is a sample source code which you can copy, paste and modify to integrate Yo Payments with your system.

### Prerequisites

To use the API, you must, first of all, have a Yo! Payments Business Account. The API is not available for Personal Accounts

* Yo! Payments API Username
* Yo! Payments API Password

### Using the library

You need to set the following.

```
string publicKeyFile = "";//This is the path to the public key certificate - Used when verifying the signature
string privateKeyFile = "";//This is the private key to be used when generating the signature to be used when 
string api_username = "yoapiusername";
string api_password = "yoapipassword"; 
string url = "https://sandbox.yo.co.ug/services/yopaymentsdev/task.php"; //For production, you need to use https://paymentsapi1.yo.co.ug/ybs/task.php

```

Note: Refer to chapter 4.2 of the main documentation for how to generate private and public key files

## Built With

* [C#](https://docs.microsoft.com/en-us/dotnet/csharp/) - C# Programming Language 

## Authors

* **Joseph Tabajjwa** - *Initial work* - [Yo (U) Ltd](https://github.com/YO-Uganda)

## License
Copyright © 2006 - 2020 YO UGANDA LIMITED All rights reserved
Yo! Payments: API Specification
Changes may be made periodically to the information in this publication. Such changes will be
incorporated in new editions of the specification. The service described in this document is made
available under a license agreement, and may be used only in accordance with the terms thereof. It is
against the law to use the service except as specifically provided in the license agreement. No part of this
publication may be reproduced, stored in a retrieval system, or transmitted in any form or by any means,
electronic, mechanical, photocopied, recorded or otherwise, without the prior written permission of Yo
Uganda Limited.
The service license is hereby incorporated herein by this reference.
All product names mentioned in this manual are for identification purposes only, and are either
trademarks or registered trademarks of their respective owners.
