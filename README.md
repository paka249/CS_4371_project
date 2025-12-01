# Introduction #

Precision medicine is an emerging approach for disease treatment and prevention that delivers
personalized care to individual patients by considering their genetic makeups, medical histories,
environments, and lifestyles. Despite the rapid advancement of precision medicine and its
considerable promise, several underlying technological challenges remain unsolved. One such
challenge of great importance is the security and privacy of precision health–related data, such as
genomic data and electronic health records, which stifle collaboration and hamper the full potential
of machine-learning (ML) algorithms. To preserve data privacy while providing ML solutions, in our
article, [Briguglio, et. al.](https://arxiv.org/abs/2102.03412), we provide three contributions.
First, we propose a generic machine learning with encryption (MLE) framework, which we used to build
an ML model that predicts cancer from one of the most recent comprehensive genomics datasets in the
field. Second, our framework’s prediction accuracy is slightly higher than that of the most recent
studies conducted on the same dataset, yet it maintains the privacy of the patients’ genomic data.
Third, to facilitate the validation, reproduction, and extension of this work, we provide an
open-source repository that contains:

* the design and implementation of the MLE framework (folder
  [`SystemArchitecture`](./SystemArchitecture)). Please, read below for more information.
* all the ML experiments and code (folder [`ModelTraining`](./ModelTraining))
* the final predictive model deployed and the MLE framework, both deployed to a free cloud service
  [`https://mle.isot.ca`](https://mle.isot.ca)

## Project Extensions

This repository has been extended with:

* **Dual encryption scheme support**: Both BFV (exact integer arithmetic) and CKKS (approximate floating-point arithmetic) homomorphic encryption schemes using Microsoft SEAL library
* **Comprehensive testing**: Multiple test datasets (binary, mixed, and high-precision synthetic genomic data) with documented expected outputs for both encryption schemes
* **Performance analysis**: Comparative evaluation of BFV vs CKKS trade-offs in accuracy, speed, and security

# Getting Started #

## Prerequisites

* **.NET Core 3.1 SDK**: Download from [Microsoft](https://dotnet.microsoft.com/download/dotnet/3.1)
* **MongoDB 4.4.25+**: Required for model coefficient storage
* **Microsoft SEAL Library**: Included via NuGet packages
* **Windows OS**: Client application tested on Windows (PowerShell)

## Clone and Build

1. **Clone the repository**:
   ```bash
   git clone https://github.com/paka249/CS_4371_project.git
   cd CS_4371_project
   ```

2. **Configure MongoDB** (ensure MongoDB is running on default port 27017):
   ```bash
   # Load model coefficients into MongoDB
   cd SystemArchitecture/configDB
   python loadModelFromCsv.py
   ```

3. **Build the Server**:
   ```bash
   cd SystemArchitecture/Server
   dotnet build
   ```

4. **Build the Client**:
   ```bash
   cd SystemArchitecture/Client
   dotnet build
   ```

## Running the Application

1. **Start the Server** (in `SystemArchitecture/Server`):
   ```bash
   dotnet run
   ```
   Server will start at `https://localhost:5001`

2. **Configure Encryption Scheme** (both Client and Server):
   
   Edit `SystemArchitecture/Client/Logics/ContextManager.cs` and `SystemArchitecture/Server/Services/ContextManager.cs`:
   
   * For **BFV** (exact integer arithmetic): Uncomment the BFV section, comment the CKKS section
   * For **CKKS** (approximate floating-point): Uncomment the CKKS section, comment the BFV section
   
   **Important**: Both client and server must use the same scheme. Restart the server after changing schemes.

3. **Run the Client** (in `SystemArchitecture/Client`):
   ```bash
   dotnet run
   ```
   
   The client will:
   * Prompt for which test file to use (testReal1.csv, testReal2.csv, or testReal3.csv)
   * Encrypt the genomic data using the selected scheme
   * Send encrypted data to the server
   * Receive and decrypt the cancer prediction results
   * Save results to `Result.csv`

## Functionality

### What Works ✅

* **BFV Encryption Scheme**: Exact integer arithmetic, faster performance (~2x), all test files produce correct predictions
* **CKKS Encryption Scheme**: Approximate floating-point arithmetic, higher precision management, all test files supported
* **Cancer Prediction**: 22 cancer classes based on 5,600 gene expression features
Encrypted Computation: Dot product calculation performed on encrypted data server-side
Multiple Test Datasets:
   `testReal1.csv`: Binary feature data (mostly 0s and 1s)
   `testReal2.csv`: Mixed binary and decimal features
   `testReal3.csv`: High-precision synthetic data 
   
### Known Limitations

* **Scheme Disagreement**: BFV and CKKS may predict different cancer types for borderline cases due to CKKS's approximate arithmetic accumulating rounding errors over 5,600 operations. BFV is more trustworthy for critical medical predictions.
* **Manual Scheme Selection**: User must manually comment/uncomment code in both client and server ContextManager files to switch encryption schemes
* **Performance**: CKKS is approximately 4x slower than BFV due to larger polynomial degree (4096 vs 2048) and additional rescaling operations
* **Scaling Factor Sensitivity**: CKKS requires careful scaling factor management (currently ÷1000) to avoid underflow with small feature values


### Security Considerations

* **Homomorphic Encryption**: Patient genomic data remains encrypted throughout transmission and computation
* **BFV Security**: ~100-bit security level (polyModulusDegree=2048)
* **CKKS Security**: ~128-bit security level (polyModulusDegree=4096)
* **MongoDB**: Model coefficients stored unencrypted (not patient data)

# System Architecture of MLE Framework #

The server is meant to be deployed as a service, hence referred to as MLE.service, which is not
exposed to the network. Instead, nginx (or something similar) should be used as a reverse proxy
which manages incoming HTTP traffic and forwards the appropriate HTTP traffic to the MLE service.
The nginx reverse proxy is also deployed as a service, nginx.service. Below is a summary for
maintenance after deploying on a Ubuntu machine with nginx reverse proxy. Instructions for
deployment can be found
[here](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx?view=aspnetcore-5.0).

## Running and configuring the services/proxy ##

You can stop or restart a service, or check a service's status (while it is running or stopped) using:

    sudo systemctl [restart|stop|status] [MLE|nginx]

The MLE service can be configured by editing `/etc/systemd/system/MLE.service`. After making edits,
the service will have to be reloaded with:

    systemctl daemon-reload

An exmple .NET service configuration can be found [here](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx?view=aspnetcore-5.0#create-the-service-file). The `ExecStart` line should read:

    ExecStart=/usr/bin/dotnet /var/www/MLE/CDTS_PROJECT.dll

The `nginx` rules for the MLE service can be configured by editing
`/etc/nginx/sites-available/default`. The global `nginx` rules, for all services, can be configured
by editing `/etc/nginx/nginx.conf`. Likewise, after making edits run:

    sudo nginx -t

Which verifies the syntax of the configuration files, and

    sudo nginx -s reload

An exmple nginx service configuration can be found [here](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx?view=aspnetcore-5.0#configure-nginx).

## Redeploying after making code changes ##

`/Server` contains the server side applications

From the Server directory, do the following:

* compile with:

        dotnet publish --configuration Release

* After compilation, copy the entire publishing folder into `/var/www/MLE` with:

        sudo rm /var/www/MLE/ -r
        sudo cp ./bin/Release/netcoreapp3.1/publish/ /var/www/MLE/ -r

* Then restart the service with:

        sudo systemctl restart MLE.service


`/Client` contains the client application

From the Client directory, do the following:

* clone the repo to a Windows machine with .NET install 3.1 installed, compile with:

        dotnet publish --configuration Release -r win-x64 -p:PublishSingleFile=true --self-contained true

* The compiled `exe` will be named `CDTS_Project.exe` in
  `./Client/bin/release/netcoreapp3.1/win-x64/publish/`; rename it `MLE.txt` and copy to both
  `/home/[username]/Healthcare-Security-Analysis/Server/wwwroot/DownloadableFiles/` and
  `/var/www/MLE/wwwroot/DownloadableFiles/`; overwrite the old files if needed with:

        cp /home/[username]/Healthcare-Security-Analysis/Client/bin/Release/netcoreapp3.1/win-x64/publish/CDTS_PROJECT.exe /home/[username]/Healthcare-Security-Analysis/Server/wwwroot/DownloadableFiles/MLE.txt

        cp /home/[username]/Healthcare-Security-Analysis/Client/bin/Release/netcoreapp3.1/win-x64/publish/CDTS_PROJECT.exe /var/www/MLE/wwwroot/DownloadableFiles/MLE.txt

* Then restart the service with:

        sudo systemctl restart MLE.service

# References #


## Current Work (Foundation for This Project)

* **Briguglio, W., Moghaddam, P., Yousef, W. A., Traore, I., & Mamun, M. (2021).** "Machine Learning in Precision Medicine to Preserve Privacy via Encryption." *arXiv Preprint, arXiv:2102.03412*. [https://arxiv.org/abs/2102.03412](https://arxiv.org/abs/2102.03412)
  
  *This paper provides the foundational MLE framework that this project is based upon, proposing a generic approach for privacy-preserving cancer prediction using homomorphic encryption on genomic datasets while maintaining competitive prediction accuracy.*

## Contemporary Work (Building Upon Current Findings)

* **Chen, H., Dai, W., Kim, M., & Song, Y. (2023).** "Efficient Homomorphic Conversion Between (Ring) LWE Ciphertexts." *Applied Cryptography and Network Security (ACNS 2023)*, Lecture Notes in Computer Science, vol 13905. [DOI: 10.1007/978-3-031-33488-7_9](https://doi.org/10.1007/978-3-031-33488-7_9)
  
  *This contemporary work extends homomorphic encryption frameworks by enabling efficient conversion between different encryption schemes (like BFV and CKKS), addressing one of the key limitations identified in current privacy-preserving ML systems. Their techniques could eliminate the manual scheme selection requirement in implementations like ours, enabling dynamic scheme switching based on computational requirements.*

# Citation # (original citation)
Please, cite this work as

```
@Article{Briguglio2021MachineLearningPrecisionMedicine-arxiv,
  author =       {William Briguglio and Parisa Moghaddam and Waleed A. Yousef and Issa Traore and
                  Mohammad Mamun},
  title =        {Machine Learning in Precision Medicine to Preserve Privacy via Encryption},
  journal =      {arXiv Preprint, arXiv:2102.03412},
  year =         2021,
  url =          {https://github.com/isotlaboratory/Healthcare-Security-Analysis-MLE},
  primaryclass = {cs.LG}
}
```

