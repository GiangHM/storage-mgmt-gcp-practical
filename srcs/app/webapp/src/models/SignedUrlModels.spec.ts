import { describe, it, expect } from 'vitest';
import SignedUrlRequestModel from './SignedUrlRequestModel';
import SignedUrlResponseModel from './SignedUrlResponseModel';

describe('SignedUrlRequestModel', () => {
  describe('Constructor', () => {
    it('Constructor_ValidParameters_CreatesInstance', () => {
      // Arrange & Act
      const model = new SignedUrlRequestModel('doc123', 'test.pdf');

      // Assert
      expect(model).toBeInstanceOf(SignedUrlRequestModel);
    });

    it('Constructor_ValidParameters_SetsProperties', () => {
      // Arrange
      const documentId = 'doc456';
      const fileName = 'document.docx';

      // Act
      const model = new SignedUrlRequestModel(documentId, fileName);

      // Assert
      expect(model.documentId).toBe('doc456');
      expect(model.fileName).toBe('document.docx');
    });

    it('Constructor_EmptyDocumentId_StoresEmptyString', () => {
      // Arrange & Act
      const model = new SignedUrlRequestModel('', 'file.txt');

      // Assert
      expect(model.documentId).toBe('');
      expect(model.fileName).toBe('file.txt');
    });

    it('Constructor_EmptyFileName_StoresEmptyString', () => {
      // Arrange & Act
      const model = new SignedUrlRequestModel('doc789', '');

      // Assert
      expect(model.documentId).toBe('doc789');
      expect(model.fileName).toBe('');
    });

    it('Constructor_BothEmpty_StoresEmptyStrings', () => {
      // Arrange & Act
      const model = new SignedUrlRequestModel('', '');

      // Assert
      expect(model.documentId).toBe('');
      expect(model.fileName).toBe('');
    });

    it('Constructor_SpecialCharactersInDocumentId_StoresCorrectly', () => {
      // Arrange
      const documentId = 'doc-123_456';

      // Act
      const model = new SignedUrlRequestModel(documentId, 'file.pdf');

      // Assert
      expect(model.documentId).toBe('doc-123_456');
    });

    it('Constructor_SpecialCharactersInFileName_StoresCorrectly', () => {
      // Arrange
      const fileName = 'file with spaces & special-chars_2024.pdf';

      // Act
      const model = new SignedUrlRequestModel('doc123', fileName);

      // Assert
      expect(model.fileName).toBe('file with spaces & special-chars_2024.pdf');
    });

    it('Constructor_LongDocumentId_StoresCorrectly', () => {
      // Arrange
      const documentId = 'a'.repeat(1000);

      // Act
      const model = new SignedUrlRequestModel(documentId, 'file.pdf');

      // Assert
      expect(model.documentId).toBe(documentId);
      expect(model.documentId.length).toBe(1000);
    });

    it('Constructor_LongFileName_StoresCorrectly', () => {
      // Arrange
      const fileName = 'b'.repeat(500) + '.pdf';

      // Act
      const model = new SignedUrlRequestModel('doc123', fileName);

      // Assert
      expect(model.fileName).toBe(fileName);
      expect(model.fileName.length).toBe(504);
    });

    it('Constructor_NumericStrings_StoresCorrectly', () => {
      // Arrange & Act
      const model = new SignedUrlRequestModel('12345', '67890');

      // Assert
      expect(model.documentId).toBe('12345');
      expect(model.fileName).toBe('67890');
    });

    it('Constructor_WhitespaceOnlyStrings_StoresCorrectly', () => {
      // Arrange & Act
      const model = new SignedUrlRequestModel('   ', '   ');

      // Assert
      expect(model.documentId).toBe('   ');
      expect(model.fileName).toBe('   ');
    });

    it('Constructor_UnicodeCharacters_StoresCorrectly', () => {
      // Arrange & Act
      const model = new SignedUrlRequestModel('doc-日本語', 'файл.pdf');

      // Assert
      expect(model.documentId).toBe('doc-日本語');
      expect(model.fileName).toBe('файл.pdf');
    });

    it('Constructor_MultipleInstances_AreIndependent', () => {
      // Arrange & Act
      const model1 = new SignedUrlRequestModel('doc1', 'file1.pdf');
      const model2 = new SignedUrlRequestModel('doc2', 'file2.pdf');

      // Assert
      expect(model1.documentId).toBe('doc1');
      expect(model2.documentId).toBe('doc2');
      expect(model1.fileName).toBe('file1.pdf');
      expect(model2.fileName).toBe('file2.pdf');
    });
  });

  describe('Properties', () => {
    it('Properties_CanBeModifiedAfterConstruction', () => {
      // Arrange
      const model = new SignedUrlRequestModel('doc123', 'file.pdf');

      // Act
      model.documentId = 'doc456';
      model.fileName = 'newfile.docx';

      // Assert
      expect(model.documentId).toBe('doc456');
      expect(model.fileName).toBe('newfile.docx');
    });

    it('Properties_CanBeSetToNull', () => {
      // Arrange
      const model = new SignedUrlRequestModel('doc123', 'file.pdf');

      // Act
      model.documentId = null as any;
      model.fileName = null as any;

      // Assert
      expect(model.documentId).toBeNull();
      expect(model.fileName).toBeNull();
    });

    it('Properties_CanBeSetToEmptyString', () => {
      // Arrange
      const model = new SignedUrlRequestModel('doc123', 'file.pdf');

      // Act
      model.documentId = '';
      model.fileName = '';

      // Assert
      expect(model.documentId).toBe('');
      expect(model.fileName).toBe('');
    });
  });
});

describe('SignedUrlResponseModel', () => {
  describe('Constructor', () => {
    it('Constructor_NoParameters_CreatesInstance', () => {
      // Arrange & Act
      const model = new SignedUrlResponseModel();

      // Assert
      expect(model).toBeInstanceOf(SignedUrlResponseModel);
    });

    it('Constructor_DefaultProperties_AreEmptyStrings', () => {
      // Arrange & Act
      const model = new SignedUrlResponseModel();

      // Assert
      expect(model.uploadUrl).toBe('');
      expect(model.objectName).toBe('');
    });

    it('Constructor_MultipleInstances_AreIndependent', () => {
      // Arrange & Act
      const model1 = new SignedUrlResponseModel();
      const model2 = new SignedUrlResponseModel();

      // Assert
      expect(model1).not.toBe(model2);
      expect(model1.uploadUrl).toBe('');
      expect(model2.uploadUrl).toBe('');
    });
  });

  describe('Properties', () => {
    it('Properties_CanSetUploadUrl', () => {
      // Arrange
      const model = new SignedUrlResponseModel();
      const uploadUrl = 'https://storage.googleapis.com/bucket/path?signature=abc123';

      // Act
      model.uploadUrl = uploadUrl;

      // Assert
      expect(model.uploadUrl).toBe(uploadUrl);
    });

    it('Properties_CanSetObjectName', () => {
      // Arrange
      const model = new SignedUrlResponseModel();
      const objectName = 'documents/doc123/test.pdf';

      // Act
      model.objectName = objectName;

      // Assert
      expect(model.objectName).toBe(objectName);
    });

    it('Properties_CanSetBothProperties', () => {
      // Arrange
      const model = new SignedUrlResponseModel();
      const uploadUrl = 'https://storage.googleapis.com/bucket/path?signature=xyz789';
      const objectName = 'documents/doc456/file.docx';

      // Act
      model.uploadUrl = uploadUrl;
      model.objectName = objectName;

      // Assert
      expect(model.uploadUrl).toBe(uploadUrl);
      expect(model.objectName).toBe(objectName);
    });

    it('Properties_CanSetEmptyStrings', () => {
      // Arrange
      const model = new SignedUrlResponseModel();
      model.uploadUrl = 'https://example.com';
      model.objectName = 'documents/doc123/file.pdf';

      // Act
      model.uploadUrl = '';
      model.objectName = '';

      // Assert
      expect(model.uploadUrl).toBe('');
      expect(model.objectName).toBe('');
    });

    it('Properties_UploadUrlCanBeGcsUrl', () => {
      // Arrange
      const model = new SignedUrlResponseModel();
      const gcsUrl = 'https://storage.googleapis.com/my-bucket/uploads/doc123?X-Goog-Algorithm=GOOG4-RSA-SHA256&X-Goog-Credential=test';

      // Act
      model.uploadUrl = gcsUrl;

      // Assert
      expect(model.uploadUrl).toContain('storage.googleapis.com');
      expect(model.uploadUrl).toContain('X-Goog-Algorithm');
    });

    it('Properties_ObjectNameFollowsGcsConvention', () => {
      // Arrange
      const model = new SignedUrlResponseModel();
      const objectName = 'documents/invoice/2024/doc-123/invoice-001.pdf';

      // Act
      model.objectName = objectName;

      // Assert
      expect(model.objectName).toBe(objectName);
      expect(model.objectName).toContain('/');
    });

    it('Properties_UploadUrlCanBeNull', () => {
      // Arrange
      const model = new SignedUrlResponseModel();

      // Act
      model.uploadUrl = null as any;

      // Assert
      expect(model.uploadUrl).toBeNull();
    });

    it('Properties_ObjectNameCanBeNull', () => {
      // Arrange
      const model = new SignedUrlResponseModel();

      // Act
      model.objectName = null as any;

      // Assert
      expect(model.objectName).toBeNull();
    });

    it('Properties_UploadUrlWithSpecialCharacters_StoresCorrectly', () => {
      // Arrange
      const model = new SignedUrlResponseModel();
      const urlWithSpecialChars = 'https://storage.googleapis.com/bucket/path?key=value&signature=abc+123=';

      // Act
      model.uploadUrl = urlWithSpecialChars;

      // Assert
      expect(model.uploadUrl).toBe(urlWithSpecialChars);
    });

    it('Properties_ObjectNameWithSpecialCharacters_StoresCorrectly', () => {
      // Arrange
      const model = new SignedUrlResponseModel();
      const objectNameWithSpecial = 'documents/file-with-dash_underscore.2024.pdf';

      // Act
      model.objectName = objectNameWithSpecial;

      // Assert
      expect(model.objectName).toBe(objectNameWithSpecial);
    });

    it('Properties_VeryLongUrls_HandledCorrectly', () => {
      // Arrange
      const model = new SignedUrlResponseModel();
      const longUrl = 'https://storage.googleapis.com/' + 'a'.repeat(2000) + '?signature=abc';

      // Act
      model.uploadUrl = longUrl;

      // Assert
      expect(model.uploadUrl).toBe(longUrl);
      expect(model.uploadUrl.length).toBeGreaterThan(2000);
    });

    it('Properties_MultiplePropertyUpdates_TracksLatestValue', () => {
      // Arrange
      const model = new SignedUrlResponseModel();

      // Act
      model.uploadUrl = 'https://example.com/1';
      model.uploadUrl = 'https://example.com/2';
      model.uploadUrl = 'https://example.com/3';

      // Assert
      expect(model.uploadUrl).toBe('https://example.com/3');
    });
  });
});
