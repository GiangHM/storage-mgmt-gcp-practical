export default class SignedUrlRequestModel {
    documentId: string;
    fileName: string;

    constructor(documentId: string, fileName: string) {
        this.documentId = documentId;
        this.fileName = fileName;
    }
}
