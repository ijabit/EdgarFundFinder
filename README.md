# Company Funding Finder

## Description

Company Funding Finder is a web application designed to analyze financial data from companies and calculate their fundable amounts based on specific criteria. This application is built with ASP.NET Core and utilizes Azure Cosmos DB for data storage and retrieval. It provides a RESTful API endpoint to fetch company funding information based on the company's name.

## Features

- **RESTful API**: The application exposes a RESTful API that allows clients to query company funding information.
- **Dynamic Querying**: It supports dynamic querying based on the company's name or ID, ensuring flexibility in data retrieval.
- **Performance Optimization**: The application is optimized for performance, leveraging the best practices for using Azure Cosmos DB, including efficient querying and connection management.
- **Fundable Amount Calculation**: It calculates the standard and special fundable amounts for companies based on their financial data, adhering to specific rules and criteria.

## Tech Stack

- **ASP.NET Core**: The application is built using ASP.NET Core, a popular framework for building web applications.
- **Azure Cosmos DB**: The application uses Azure Cosmos DB for storing and retrieving financial data.
- **C#**: The application is written in C#, a versatile programming language widely used for web development.

## How to Run the Project

1. **Clone the Repository**: Clone the repository to your local machine.
2. **Restore Dependencies**: Run `dotnet restore` in the terminal to restore the project dependencies.
4. **Run the Application**: Use `dotnet run` to start the application. The application will be accessible at `http://localhost:5000`.

## API Endpoints

- **GET /company?companyName={name}**: Retrieves company funding information by name.

## Accessing the API

The project is published and accessible at [https://edgarfundfinderapi-capp.jollybush-bd14d0e1.eastus2.azurecontainerapps.io/](https://edgarfundfinderapi-capp.jollybush-bd14d0e1.eastus2.azurecontainerapps.io/). You can also access the API Swagger document at [https://edgarfundfinderapi-capp.jollybush-bd14d0e1.eastus2.azurecontainerapps.io/swagger/index.html](https://edgarfundfinderapi-capp.jollybush-bd14d0e1.eastus2.azurecontainerapps.io/swagger/index.html) for detailed API documentation.

## Running with Docker

This project is set up to run using Docker. If you're using Visual Studio, it can take care of building and running the Docker container for you. If you don't have Docker configured on your machine, follow these steps:

1. **Install Docker**: Download and install Docker from [https://www.docker.com/products/docker-desktop](https://www.docker.com/products/docker-desktop).
2. **Build the Docker Image**: Navigate to the project directory in your terminal and run `docker build -t companyfundingfinder .` to build the Docker image.
3. **Run the Docker Container**: Run `docker run -p 5000:80 companyfundingfinder` to start the application in a Docker container. The application will be accessible at `http://localhost:5000`.

## Security Note

The CosmosDB connection string is directly embedded in `appsettings.json` temporarily. In a production-level application, this would be pulled in from a secure location such as Azure Key Vault.