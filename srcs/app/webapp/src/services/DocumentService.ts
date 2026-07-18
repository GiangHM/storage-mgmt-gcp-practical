import HttpService from "./HttpService";
import DocumentResponseModel from "../models/DocumentResponseModel";
import DocumentCreationRequestModel from "../models/DocumentCreationRequestModel";

export default class DocumentService 
{
    httpService: HttpService = new HttpService();

   
    
    async getDocumentFromAPI(): Promise<DocumentResponseModel[]>{
        return await this.httpService.get<DocumentResponseModel[]>("DocumentManagement")
    }
    async addNewDocument(model: DocumentCreationRequestModel){
        return await this.httpService.post("DocumentManagement", model)
    }
};

