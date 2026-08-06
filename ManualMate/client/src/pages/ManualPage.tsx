import { useState, useEffect } from 'react';
import { itemApi } from '../services/api';
import type { Item } from '../types';
import { BookOpen, Loader2, CheckCircle, XCircle } from 'lucide-react';

export default function ManualPage() {
  const [items, setItems] = useState<Item[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadItems();
  }, []);

  const loadItems = async () => {
    try {
      setLoading(true);
      const data = await itemApi.getAll();
      setItems(data);
      setError(null);
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to load items');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="px-4 py-6 sm:px-0 animate-fade-in">
      <div className="flex items-center mb-6">
        <BookOpen className="h-8 w-8 mr-3 text-gray-100" />
        <h1 className="text-3xl font-bold text-white">Manuals</h1>
      </div>

      <div className="bg-blue-500/20 border border-blue-500/30 rounded-xl p-4 backdrop-blur-sm mb-6">
        <p className="text-sm text-blue-300">
          A manual (PDF) is uploaded when an item is created and processed automatically by the backend.
        </p>
      </div>

      {error && (
        <div className="mb-4 bg-red-500/20 border border-red-500/30 text-red-300 px-4 py-3 rounded-xl backdrop-blur-sm">
          {error}
        </div>
      )}

      {loading ? (
        <div className="flex justify-center items-center h-64">
          <Loader2 className="h-8 w-8 animate-spin text-blue-400" />
        </div>
      ) : items.length === 0 ? (
        <div className="text-center py-12 animate-slide-up">
          <p className="text-gray-300 text-lg">No items found. Create an item with a PDF file to get started.</p>
        </div>
      ) : (
        <div className="bg-[#202123] shadow-xl overflow-hidden rounded-lg border border-white/10">
          <ul className="divide-y divide-white/10">
            {items.map((item) => (
              <li key={item.id} className="hover:bg-[#2a2b32] transition-all duration-200">
                <div className="px-4 py-4 sm:px-6">
                  <div className="flex items-start justify-between">
                    <div className="flex-1">
                      <h3 className="text-lg font-medium text-white">{item.name}</h3>
                      <p className="mt-1 text-sm text-gray-300">{item.description}</p>
                    </div>
                  </div>
                  <div className="mt-3">
                    {item.filePath ? (
                      <div className="flex items-center text-sm text-green-400">
                        <CheckCircle className="h-4 w-4 mr-2 flex-shrink-0" />
                        <span className="truncate">File: {item.filePath}</span>
                      </div>
                    ) : (
                      <div className="flex items-center text-sm text-red-400">
                        <XCircle className="h-4 w-4 mr-2 flex-shrink-0" />
                        No manual uploaded
                      </div>
                    )}
                  </div>
                </div>
              </li>
            ))}
          </ul>
        </div>
      )}
    </div>
  );
}
