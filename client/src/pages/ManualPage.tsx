import { useState, useEffect } from 'react';
import { itemApi } from '../services/api';
import type { Item } from '../types';
import { Upload, Play, Loader2, FileText, CheckCircle, XCircle, Trash2 } from 'lucide-react';

export default function ManualPage() {
  const [items, setItems] = useState<Item[]>([]);
  const [selectedItem, setSelectedItem] = useState<Item | null>(null);
  const [file, setFile] = useState<File | null>(null);
  const [uploading, setUploading] = useState(false);
  const [processing, setProcessing] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [uploadStatus, setUploadStatus] = useState<{ type: 'success' | 'error'; message: string } | null>(null);
  const [processingStatus, setProcessingStatus] = useState<{ type: 'success' | 'error'; message: string } | null>(null);
  const [deleteStatus, setDeleteStatus] = useState<{ type: 'success' | 'error'; message: string } | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadItems();
  }, []);

  const loadItems = async () => {
    try {
      setLoading(true);
      const data = await itemApi.getAll();
      setItems(data);
      if (data.length > 0 && !selectedItem) {
        setSelectedItem(data[0]);
      }
    } catch (err: any) {
      setUploadStatus({ type: 'error', message: err.response?.data?.error || 'Failed to load items' });
    } finally {
      setLoading(false);
    }
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files[0]) {
      const selectedFile = e.target.files[0];
      if (selectedFile.type === 'application/pdf') {
        setFile(selectedFile);
        setUploadStatus(null);
      } else {
        setUploadStatus({ type: 'error', message: 'Please select a PDF file' });
        setFile(null);
      }
    }
  };

  const handleUpload = async () => {
    if (!selectedItem || !file) {
      setUploadStatus({ type: 'error', message: 'Please select an item and a PDF file' });
      return;
    }

    setUploading(true);
    setUploadStatus(null);

    try {
      await itemApi.uploadFile(selectedItem.id, file);
      setUploadStatus({ type: 'success', message: 'Manual uploaded successfully!' });
      setFile(null);
      await loadItems();
    } catch (err: any) {
      setUploadStatus({ type: 'error', message: err.response?.data?.error || 'Failed to upload manual' });
    } finally {
      setUploading(false);
    }
  };

  const handleProcess = async () => {
    if (!selectedItem) {
      setProcessingStatus({ type: 'error', message: 'Please select an item' });
      return;
    }

    if (!selectedItem.filePath) {
      setProcessingStatus({ type: 'error', message: 'This item has no manual uploaded. Please upload a manual first.' });
      return;
    }

    setProcessing(true);
    setProcessingStatus(null);

    try {
      await itemApi.processFile(selectedItem.id);
      setProcessingStatus({ type: 'success', message: 'Manual processed successfully! Embeddings have been created.' });
      await loadItems();
    } catch (err: any) {
      setProcessingStatus({ type: 'error', message: err.response?.data?.error || 'Failed to process manual' });
    } finally {
      setProcessing(false);
    }
  };

  const handleDeleteEmbeddings = async () => {
    if (!selectedItem) {
      setDeleteStatus({ type: 'error', message: 'Please select an item' });
      return;
    }

    if (!confirm(`Are you sure you want to delete all embeddings for "${selectedItem.name}"? This action cannot be undone.`)) {
      return;
    }

    setDeleting(true);
    setDeleteStatus(null);

    try {
      await itemApi.deleteEmbeddings(selectedItem.id);
      setDeleteStatus({ type: 'success', message: 'Embeddings deleted successfully!' });
      await loadItems();
    } catch (err: any) {
      setDeleteStatus({ type: 'error', message: err.response?.data?.error || 'Failed to delete embeddings' });
    } finally {
      setDeleting(false);
    }
  };

  return (
    <div className="px-4 py-6 sm:px-0">
      <h1 className="text-3xl font-bold text-gray-900 mb-6">Manual Management</h1>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Upload Section */}
        <div className="bg-white shadow rounded-lg p-6">
          <h2 className="text-xl font-semibold text-gray-900 mb-4 flex items-center">
            <Upload className="h-5 w-5 mr-2 text-primary-600" />
            Upload Manual
          </h2>

          <div className="space-y-4">
            <div>
              <label htmlFor="product-select" className="block text-sm font-medium text-gray-700 mb-2">
                Select Item
              </label>
              <select
                id="product-select"
                value={selectedItem?.id || ''}
                onChange={(e) => {
                  const item = items.find(p => p.id === parseInt(e.target.value));
                  setSelectedItem(item || null);
                }}
                className="block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-primary-500 focus:border-primary-500"
                disabled={loading}
              >
                {items.map((item) => (
                  <option key={item.id} value={item.id}>
                    {item.name}
                  </option>
                ))}
              </select>
            </div>

            <div>
              <label htmlFor="file-upload" className="block text-sm font-medium text-gray-700 mb-2">
                PDF Manual File
              </label>
              <div className="mt-1 flex justify-center px-6 pt-5 pb-6 border-2 border-gray-300 border-dashed rounded-md hover:border-primary-400 transition-colors">
                <div className="space-y-1 text-center">
                  <FileText className="mx-auto h-12 w-12 text-gray-400" />
                  <div className="flex text-sm text-gray-600">
                    <label htmlFor="file-upload" className="relative cursor-pointer bg-white rounded-md font-medium text-primary-600 hover:text-primary-500 focus-within:outline-none focus-within:ring-2 focus-within:ring-offset-2 focus-within:ring-primary-500">
                      <span>Upload a file</span>
                      <input
                        id="file-upload"
                        name="file-upload"
                        type="file"
                        accept=".pdf"
                        className="sr-only"
                        onChange={handleFileChange}
                      />
                    </label>
                    <p className="pl-1">or drag and drop</p>
                  </div>
                  <p className="text-xs text-gray-500">PDF up to 10MB</p>
                  {file && (
                    <p className="text-sm text-primary-600 mt-2">{file.name}</p>
                  )}
                </div>
              </div>
            </div>

            {uploadStatus && (
              <div className={`flex items-center p-3 rounded-md ${uploadStatus.type === 'success'
                ? 'bg-green-50 text-green-800'
                : 'bg-red-50 text-red-800'
                }`}>
                {uploadStatus.type === 'success' ? (
                  <CheckCircle className="h-5 w-5 mr-2" />
                ) : (
                  <XCircle className="h-5 w-5 mr-2" />
                )}
                <span>{uploadStatus.message}</span>
              </div>
            )}

            <button
              onClick={handleUpload}
              disabled={uploading || !file || !selectedItem}
              className="w-full inline-flex justify-center items-center px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-primary-600 hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {uploading ? (
                <>
                  <Loader2 className="h-5 w-5 mr-2 animate-spin" />
                  Uploading...
                </>
              ) : (
                <>
                  <Upload className="h-5 w-5 mr-2" />
                  Upload Manual
                </>
              )}
            </button>
          </div>
        </div>

        {/* Process Section */}
        <div className="bg-white shadow rounded-lg p-6">
          <h2 className="text-xl font-semibold text-gray-900 mb-4 flex items-center">
            <Play className="h-5 w-5 mr-2 text-primary-600" />
            Process Manual
          </h2>

          <div className="space-y-4">
            <div>
              <label htmlFor="process-product-select" className="block text-sm font-medium text-gray-700 mb-2">
                Select Item
              </label>
              <select
                id="process-product-select"
                value={selectedItem?.id || ''}
                onChange={(e) => {
                  const item = items.find(p => p.id === parseInt(e.target.value));
                  setSelectedItem(item || null);
                }}
                className="block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-primary-500 focus:border-primary-500"
                disabled={loading}
              >
                {items.map((item) => (
                  <option key={item.id} value={item.id}>
                    {item.name} {item.filePath ? '✓' : ''}
                  </option>
                ))}
              </select>
            </div>

            {selectedItem && (
              <div className="bg-gray-50 p-4 rounded-md">
                <p className="text-sm text-gray-600">
                  <span className="font-medium">Item:</span> {selectedItem.name}
                </p>
                <p className="text-sm text-gray-600 mt-1">
                  <span className="font-medium">Manual:</span>{' '}
                  {selectedItem.filePath ? (
                    <span className="text-green-600">{selectedItem.filePath}</span>
                  ) : (
                    <span className="text-red-600">No manual uploaded</span>
                  )}
                </p>
              </div>
            )}

            {processingStatus && (
              <div className={`flex items-center p-3 rounded-md ${processingStatus.type === 'success'
                ? 'bg-green-50 text-green-800'
                : 'bg-red-50 text-red-800'
                }`}>
                {processingStatus.type === 'success' ? (
                  <CheckCircle className="h-5 w-5 mr-2" />
                ) : (
                  <XCircle className="h-5 w-5 mr-2" />
                )}
                <span>{processingStatus.message}</span>
              </div>
            )}

            <button
              onClick={handleProcess}
              disabled={processing || !selectedItem || !selectedItem?.filePath}
              className="w-full inline-flex justify-center items-center px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-green-600 hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-green-500 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {processing ? (
                <>
                  <Loader2 className="h-5 w-5 mr-2 animate-spin" />
                  Processing...
                </>
              ) : (
                <>
                  <Play className="h-5 w-5 mr-2" />
                  Process Manual & Create Embeddings
                </>
              )}
            </button>

            <div className="bg-blue-50 border border-blue-200 rounded-md p-4">
              <p className="text-sm text-blue-800">
                <strong>Note:</strong> Processing a manual will extract text, chunk it, generate embeddings, and store them in the database. This may take a few minutes depending on the manual size.
              </p>
            </div>
          </div>
        </div>

        {/* Delete Embeddings Section */}
        <div className="bg-white shadow rounded-lg p-6">
          <h2 className="text-xl font-semibold text-gray-900 mb-4 flex items-center">
            <Trash2 className="h-5 w-5 mr-2 text-red-600" />
            Delete Embeddings
          </h2>

          <div className="space-y-4">
            <div>
              <label htmlFor="delete-product-select" className="block text-sm font-medium text-gray-700 mb-2">
                Select Item
              </label>
              <select
                id="delete-product-select"
                value={selectedItem?.id || ''}
                onChange={(e) => {
                  const item = items.find(p => p.id === parseInt(e.target.value));
                  setSelectedItem(item || null);
                }}
                className="block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-primary-500 focus:border-primary-500"
                disabled={loading}
              >
                {items.map((item) => (
                  <option key={item.id} value={item.id}>
                    {item.name}
                  </option>
                ))}
              </select>
            </div>

            {selectedItem && (
              <div className="bg-gray-50 p-4 rounded-md">
                <p className="text-sm text-gray-600">
                  <span className="font-medium">Item:</span> {selectedItem.name}
                </p>
                <p className="text-sm text-gray-600 mt-1">
                  <span className="font-medium">Description:</span> {selectedItem.description}
                </p>
              </div>
            )}

            {deleteStatus && (
              <div className={`flex items-center p-3 rounded-md ${deleteStatus.type === 'success'
                ? 'bg-green-50 text-green-800'
                : 'bg-red-50 text-red-800'
                }`}>
                {deleteStatus.type === 'success' ? (
                  <CheckCircle className="h-5 w-5 mr-2" />
                ) : (
                  <XCircle className="h-5 w-5 mr-2" />
                )}
                <span>{deleteStatus.message}</span>
              </div>
            )}

            <button
              onClick={handleDeleteEmbeddings}
              disabled={deleting || !selectedItem}
              className="w-full inline-flex justify-center items-center px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-red-600 hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {deleting ? (
                <>
                  <Loader2 className="h-5 w-5 mr-2 animate-spin" />
                  Deleting...
                </>
              ) : (
                <>
                  <Trash2 className="h-5 w-5 mr-2" />
                  Delete All Embeddings
                </>
              )}
            </button>

            <div className="bg-red-50 border border-red-200 rounded-md p-4">
              <p className="text-sm text-red-800">
                <strong>Warning:</strong> This will permanently delete all embeddings for the selected item. You will need to process the manual again to recreate them. This action cannot be undone.
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
