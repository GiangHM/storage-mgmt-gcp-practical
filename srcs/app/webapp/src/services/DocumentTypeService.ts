import HttpService from "./HttpService";
import DocumentTypeResponseModel from "../models/DocumentTypeResponseModel";

export default class DocumentTypeService 
{
    httpService: HttpService = new HttpService();

    
    
    async getDocumentTypeFromAPI(): Promise<DocumentTypeResponseModel[]>{
        return await this.httpService.get<DocumentTypeResponseModel[]>("DocumentType")
    }
};

