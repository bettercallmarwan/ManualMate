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
    <div className="px-4 py-6 sm:px-0 animate-fade-in">
      <h1 className="text-3xl font-bold text-white mb-6">Items Management</h1>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Upload Section */}
        <div className="bg-[#202123] shadow-xl rounded-lg p-6 border border-white/10 hover:border-white/20 transition-all duration-300">
          <h2 className="text-xl font-semibold text-white mb-4 flex items-center">
            <Upload className="h-5 w-5 mr-2 text-blue-400" />
            Upload File
          </h2>

          <div className="space-y-4">
            <div>
              <label htmlFor="product-select" className="block text-sm font-medium text-gray-300 mb-2">
                Select Item
              </label>
              <select
                id="product-select"
                value={selectedItem?.id || ''}
                onChange={(e) => {
                  const item = items.find(p => p.id === parseInt(e.target.value));
                  setSelectedItem(item || null);
                }}
                className="block w-full bg-slate-800/50 border border-slate-600 rounded-lg shadow-sm py-2 px-3 text-gray-100 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all duration-300"
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
              <label htmlFor="file-upload" className="block text-sm font-medium text-gray-300 mb-2">
                PDF Manual File
              </label>
              <div className="mt-1 flex justify-center px-6 pt-5 pb-6 border-2 border-slate-600 border-dashed rounded-xl hover:border-blue-400 hover:bg-slate-800/30 transition-all duration-300">
                <div className="space-y-1 text-center">
                  <FileText className="mx-auto h-12 w-12 text-blue-400" />
                  <div className="flex text-sm text-gray-300">
                    <label htmlFor="file-upload" className="relative cursor-pointer rounded-md font-medium text-blue-400 hover:text-blue-300 focus-within:outline-none focus-within:ring-2 focus-within:ring-offset-2 focus-within:ring-blue-500 transition-colors">
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
                  <p className="text-xs text-gray-400">PDF up to 10MB</p>
                  {file && (
                    <p className="text-sm text-blue-400 mt-2 font-medium">{file.name}</p>
                  )}
                </div>
              </div>
            </div>

            {uploadStatus && (
              <div className={`flex items-center p-3 rounded-xl backdrop-blur-sm ${uploadStatus.type === 'success'
                ? 'bg-green-500/20 text-green-300 border border-green-500/30'
                : 'bg-red-500/20 text-red-300 border border-red-500/30'
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
              className="w-full inline-flex justify-center items-center px-4 py-2.5 border-0 rounded-xl shadow-lg text-sm font-medium text-white gradient-blue hover:scale-105 hover-glow-blue focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:scale-100 transition-all duration-300"
            >
              {uploading ? (
                <>
                  <Loader2 className="h-5 w-5 mr-2 animate-spin" />
                  Uploading...
                </>
              ) : (
                <>
                  <Upload className="h-5 w-5 mr-2" />
                  Upload File
                </>
              )}
            </button>
          </div>
        </div>

        {/* Process Section */}
        <div className="bg-[#202123] shadow-xl rounded-lg p-6 border border-white/10 hover:border-white/20 transition-all duration-300">
          <h2 className="text-xl font-semibold text-white mb-4 flex items-center">
            <Play className="h-5 w-5 mr-2 text-green-400" />
            Process File
          </h2>

          <div className="space-y-4">
            <div>
              <label htmlFor="process-product-select" className="block text-sm font-medium text-gray-300 mb-2">
                Select Item
              </label>
              <select
                id="process-product-select"
                value={selectedItem?.id || ''}
                onChange={(e) => {
                  const item = items.find(p => p.id === parseInt(e.target.value));
                  setSelectedItem(item || null);
                }}
                className="block w-full bg-slate-800/50 border border-slate-600 rounded-lg shadow-sm py-2 px-3 text-gray-100 focus:outline-none focus:ring-2 focus:ring-green-500 focus:border-transparent transition-all duration-300"
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
              <div className="bg-slate-800/30 p-4 rounded-xl border border-slate-700/50">
                <p className="text-sm text-gray-300">
                  <span className="font-medium text-white">Item:</span> {selectedItem.name}
                </p>
                <p className="text-sm text-gray-300 mt-1">
                  <span className="font-medium text-white">FIle:</span>{' '}
                  {selectedItem.filePath ? (
                    <span className="text-green-400">{selectedItem.filePath}</span>
                  ) : (
                    <span className="text-red-400">No File uploaded</span>
                  )}
                </p>
              </div>
            )}

            {processingStatus && (
              <div className={`flex items-center p-3 rounded-xl backdrop-blur-sm ${processingStatus.type === 'success'
                ? 'bg-green-500/20 text-green-300 border border-green-500/30'
                : 'bg-red-500/20 text-red-300 border border-red-500/30'
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
              className="w-full inline-flex justify-center items-center px-4 py-2.5 border-0 rounded-xl shadow-lg text-sm font-medium text-white gradient-green hover:scale-105 hover-glow-green focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-green-500 disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:scale-100 transition-all duration-300"
            >
              {processing ? (
                <>
                  <Loader2 className="h-5 w-5 mr-2 animate-spin" />
                  Processing...
                </>
              ) : (
                <>
                  <Play className="h-5 w-5 mr-2" />
                  Process File & Create Embeddings
                </>
              )}
            </button>

            <div className="bg-blue-500/20 border border-blue-500/30 rounded-xl p-4 backdrop-blur-sm">
              <p className="text-sm text-blue-300">
                <strong>Note:</strong> Processing a file will extract text, chunk it, generate embeddings, and store them in the database. This may take a few minutes depending on the file size.
              </p>
            </div>
          </div>
        </div>

        {/* Delete Embeddings Section */}
        <div className="bg-[#202123] shadow-xl rounded-lg p-6 border border-white/10 hover:border-white/20 transition-all duration-300">
          <h2 className="text-xl font-semibold text-white mb-4 flex items-center">
            <Trash2 className="h-5 w-5 mr-2 text-red-400" />
            Delete Embeddings
          </h2>

          <div className="space-y-4">
            <div>
              <label htmlFor="delete-product-select" className="block text-sm font-medium text-gray-300 mb-2">
                Select Item
              </label>
              <select
                id="delete-product-select"
                value={selectedItem?.id || ''}
                onChange={(e) => {
                  const item = items.find(p => p.id === parseInt(e.target.value));
                  setSelectedItem(item || null);
                }}
                className="block w-full bg-slate-800/50 border border-slate-600 rounded-lg shadow-sm py-2 px-3 text-gray-100 focus:outline-none focus:ring-2 focus:ring-red-500 focus:border-transparent transition-all duration-300"
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
              <div className="bg-slate-800/30 p-4 rounded-xl border border-slate-700/50">
                <p className="text-sm text-gray-300">
                  <span className="font-medium text-white">Item:</span> {selectedItem.name}
                </p>
                <p className="text-sm text-gray-300 mt-1">
                  <span className="font-medium text-white">Description:</span> {selectedItem.description}
                </p>
              </div>
            )}

            {deleteStatus && (
              <div className={`flex items-center p-3 rounded-xl backdrop-blur-sm ${deleteStatus.type === 'success'
                ? 'bg-green-500/20 text-green-300 border border-green-500/30'
                : 'bg-red-500/20 text-red-300 border border-red-500/30'
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
              className="w-full inline-flex justify-center items-center px-4 py-2.5 border-0 rounded-xl shadow-lg text-sm font-medium text-white gradient-red hover:scale-105 hover-glow-red focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500 disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:scale-100 transition-all duration-300"
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

            <div className="bg-red-500/20 border border-red-500/30 rounded-xl p-4 backdrop-blur-sm">
              <p className="text-sm text-red-300">
                <strong>Warning:</strong> This will permanently delete all embeddings for the selected item. You will need to process the file again to recreate them. This action cannot be undone.
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
