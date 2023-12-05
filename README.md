<a name="readme-top"></a>
<!--
*** Thanks for checking out the project. If you have a suggestion that would make this better, please fork the repo and create a pull request
*** Don't forget to give the project a star!
*** Thanks again!
-->
<!-- TABLE OF CONTENTS -->
<details>
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>
      <ul>
        <li><a href="#built-with">Built With</a></li>
      </ul>
    </li>
    <li>
      <a href="#getting-started">Getting Started</a>
      <ul>
        <li><a href="#requirements">Requirements</a></li>
        <li><a href="#installation">Installation</a></li>
      </ul>
    </li>
    <li><a href="#usage">Usage</a></li>## Demo
    <li><a href="#Demo">Demo</a></li>
    <li><a href="#license">License</a></li>
  </ol>
</details>

<!-- ABOUT THE PROJECT -->
## About The Project

Project: Online Auction System (OAS)<br>
Part 1: Source Code back-end.<br>
**Part 2: Source Code front-end**.<br>
This is the second part of the project.

The system supports users to participate in online product auctions.<br>
Users can post products for auction.<br>
Users can also participate in product auctions.<br>
After successfully bidding on the product, users can make online payments on the system.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

### Built With

The project is built based on the following frameworks/libraries:

* ![C#](https://img.shields.io/badge/C%23-8A2BE2.svg?style=for-the-badge&logo=C%23)
* ![dotNET Core](https://img.shields.io/badge/.NET%20Core-purple?style=for-the-badge&logo=dotNET)
* ![ASP.NET MVC](https://img.shields.io/badge/ASP.NET%20MVC-purple?style=for-the-badge&logo=dotnet)
* ![Hangfire](https://img.shields.io/badge/Hangfire-purple?style=for-the-badge&logo=dotnet)
* ![SignalR](https://img.shields.io/badge/SignalR-purple?style=for-the-badge&logo=dotnet)
* ![Redis](https://img.shields.io/badge/Redis-black?style=for-the-badge&logo=Redis)
* ![VNPAY](https://img.shields.io/badge/VNPAY-blue?style=for-the-badge&logo=VNPAY)
* ![AmazonS3](https://img.shields.io/badge/Amazon%20S3-green?style=for-the-badge&logo=Amazon%20S3)
* ![JWT](https://img.shields.io/badge/JWT-black?style=for-the-badge&logo=web%20token)
* ![RESTful API](https://img.shields.io/badge/RESTful%20API-blue?style=for-the-badge&logo=RESTful%20API)
* ![HTML5](https://img.shields.io/badge/html5%20-%23E34F26.svg?&style=for-the-badge&logo=html5&logoColor=white)
* ![CSS3](https://img.shields.io/badge/css3%20-%231572B6.svg?&style=for-the-badge&logo=css3&logoColor=white)
* ![Bootstrap](https://img.shields.io/badge/bootstrap%20-%23563D7C.svg?&style=for-the-badge&logo=bootstrap&logoColor=white)
* ![Javascript](https://img.shields.io/badge/javascript%20-%23323330.svg?&style=for-the-badge&logo=javascript&logoColor=%23F7DF1E)

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- GETTING STARTED -->
## Getting Started

To get a local copy up and running follow these steps.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

### Requirements

Before you continue, ensure you meet the following requirements:
* MS SQL Server, Version = 2022
* dotNet, Version = 6.0
* Redis, Version = 3.0.504
* SignalR, Version = 6.0.21
* Account VNPAY
* AWS IAM Account or Root Account
* Hangfire Core, Version = 1.8.5.0
* Gmail Account
* OS: Windows

<p align="right">(<a href="#readme-top">back to top</a>)</p>

### Installation

I am deploying the system on Docker but it is not yet completed, in the meantime you can install the system and components manually as follows:
* First you need to install dotNet 6.0 or later.
* After installation, next you install MS SQL Server version 2022 or later.
* You can install an IDE to code C# such as Visual Studio or another IDE, my project uses Visual Studio 2022.
* Install Redis version = 3.0.504 or later.
* Next, you can Clone the source code or Download the Zip file project OAS.FE.
* **Note**: This project only contains the front-end source code of the project, if you need the full project, you need to install the OAS.BE project in my github. I have a pin on my github homepage or you can refer to it [**here**](https://github.com/btnhutdev/OAS.BE)
* You also need to customize information such as AWS account, Email Configuration, Connection Strings, JWT Token,... in the following projects:
* Next register an account at VNPAY Payment Gateway. ([**Register**](https://sandbox.vnpayment.vn/devreg) a free account in dev/test environment).  
* You also need to customize information such as AWS account, Email Configuration, Connection Strings, JWT Token,... in the following projects:
  * Admin\appsettings.json file
  * Auctioneer\appsettings.json file
  * Bidder\appsettings.json file 
* Install AWS CLI version 2 and log in with your AWS account, making sure your account has permission to access the S3 bucket.
  
* **Note**: Some of the installation steps you performed when installing the OAS.BE project can be skipped.
<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- USAGE EXAMPLES -->
## Usage
* Right-click Solution in the Solution Explorer window, select Properties, select Multiple startup projects. Select action start for projects:
  * Admin
  * Auctioneer
  * Bidder
* You can run the project in local by clicking Start in Visual Studio.
* Access the address http://localhost:5000 to enter the Admin interface 
* Access the address http://localhost:5035 to enter the Auctioneer interface
* Access the address http://localhost:5030 to enter the Bidder interface
* **Note**: The ports above are default, you can customize them in the Properties\launchSettings.json file of each project.

<!-- DEMO -->
## Demo
<p align="center">
  <img src="" width=1000><br/>
  <i>Hangfire Dashboard</i>
</p>

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- LICENSE -->
## License
Distributed under the MIT License. See `LICENSE.txt` for more information.

<p align="right">(<a href="#readme-top">back to top</a>)</p>
