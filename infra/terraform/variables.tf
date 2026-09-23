variable "subscription_id" {
  description = "Azure Subscription ID where MiniShop will be deployed."
  type        = string
}

variable "project_name" {
  description = "Prohect name used for resource naming"
  type        = string
  default     = "minishop"
}

variable "environment" {
  description = "Environment name."
  type        = string
  default     = "tf-dev"
}


variable "location" {
  description = "Main Azure region for Container Apps, ACR and Log Analytics."
  type        = string
  default     = "eastus"
}

variable "sql_location" {
  description = "Azure region for SQL Server and SQL Database."
  type        = string
  default     = "centralus"
}

variable "sql_admin_login" {
  description = "Azure SQL administrator username."
  type        = string
  default     = "minishopadmin"
}

variable "sql_admin_password" {
  description = "Azure SQL administrator password."
  type        = string
  sensitive   = true
}

variable "sql_database_name" {
  description = "MiniShop database name."
  type        = string
  default     = "MiniShop"
}

variable "webapi_image_name" {
  description = "Docker image name for MiniShop.WebApi."
  type        = string
  default     = "minishop-webapi"
}

variable "webapi_image_tag" {
  description = "Docker image tag for MiniShop.WebApi."
  type        = string
  default     = "terraform-dev-1"
}

variable "deploy_container_app" {
  description = "Whether to deploy the Container App. Start false, push image to ACR, then set true."
  type        = bool
  default     = false
}
