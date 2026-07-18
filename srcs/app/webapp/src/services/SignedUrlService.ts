import HttpService from "./HttpService";
import SignedUrlRequestModel from "../models/SignedUrlRequestModel";
import SignedUrlResponseModel from "../models/SignedUrlResponseModel";

export default class SignedUrlService {
    private httpService: HttpService = new HttpService();

    async requestSignedUrl(documentId: string, fileName: string): Promise<SignedUrlResponseModel> {
        const requestModel = new SignedUrlRequestModel(documentId, fileName);
        return await this.httpService.postForData<SignedUrlRequestModel, SignedUrlResponseModel>(
            "SignedUrlGenerator/generate",
            requestModel
        );
    }
}
