import { useState, useEffect } from 'react';
import { itemApi } from '../services/api';
import type { Item, CreateItemDto } from '../types';
import { Plus, Edit, Trash2, Loader2 } from 'lucide-react';

export default function ItemsPage() {
  const [items, setItems] = useState<Item[]>([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [editingItem, setEditingItem] = useState<Item | null>(null);
  const [formData, setFormData] = useState<CreateItemDto>({
    name: '',
    description: '',
  });
  const [submitting, setSubmitting] = useState(false);
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

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    setError(null);

    try {
      if (editingItem) {
        await itemApi.update(editingItem.id, formData);
      } else {
        await itemApi.create(formData);
      }
      await loadItems();
      setShowModal(false);
      setEditingItem(null);
      setFormData({ name: '', description: '' });
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to save item');
    } finally {
      setSubmitting(false);
    }
  };

  const handleEdit = (item: Item) => {
    setEditingItem(item);
    setFormData({
      name: item.name,
      description: item.description,
    });
    setShowModal(true);
  };

  const handleDelete = async (id: number) => {
    if (!confirm('Are you sure you want to delete this item?')) return;

    try {
      await itemApi.delete(id);
      await loadItems();
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to delete item');
    }
  };

  const openCreateModal = () => {
    setEditingItem(null);
    setFormData({ name: '', description: '' });
    setShowModal(true);
  };

  return (
    <div className="px-4 py-6 sm:px-0 animate-fade-in">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold text-white">Items</h1>
        <button
          onClick={openCreateModal}
          className="inline-flex items-center px-4 py-2 border-0 rounded-xl shadow-lg text-sm font-medium text-white gradient-purple hover:scale-105 hover-glow-purple focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-purple-500 transition-all duration-300"
        >
          <Plus className="h-5 w-5 mr-2" />
          Add Item
        </button>
      </div>

      {error && (
        <div className="mb-4 bg-red-500/20 border border-red-500/30 text-red-300 px-4 py-3 rounded-xl backdrop-blur-sm">
          {error}
        </div>
      )}

      {loading ? (
        <div className="flex justify-center items-center h-64">
          <Loader2 className="h-8 w-8 animate-spin text-purple-400" />
        </div>
      ) : items.length === 0 ? (
        <div className="text-center py-12 animate-slide-up">
          <p className="text-gray-300 text-lg">No items found. Create your first item!</p>
        </div>
      ) : (
        <div className="bg-[#202123] shadow-xl overflow-hidden rounded-lg border border-white/10">
          <ul className="divide-y divide-white/10">
            {items.map((item) => (
              <li key={item.id} className="hover:bg-[#2a2b32] transition-all duration-200">
                <div className="px-4 py-4 sm:px-6 flex items-center justify-between">
                  <div className="flex-1">
                    <h3 className="text-lg font-medium text-white">{item.name}</h3>
                    <p className="mt-1 text-sm text-gray-300">{item.description}</p>
                    {item.filePath && (
                      <p className="mt-1 text-xs text-purple-400">
                        File: {item.filePath}
                      </p>
                    )}
                  </div>
                  <div className="flex space-x-2 ml-4">
                    <button
                      onClick={() => handleEdit(item)}
                      className="inline-flex items-center px-3 py-2 border-2 border-slate-600 shadow-sm text-sm leading-4 font-medium rounded-lg text-gray-300 bg-slate-800/50 hover:bg-slate-700/50 hover:scale-105 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 transition-all duration-300"
                    >
                      <Edit className="h-4 w-4" />
                    </button>
                    <button
                      onClick={() => handleDelete(item.id)}
                      className="inline-flex items-center px-3 py-2 border-2 border-red-600/50 shadow-sm text-sm leading-4 font-medium rounded-lg text-red-400 bg-slate-800/50 hover:bg-red-500/20 hover:scale-105 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500 transition-all duration-300"
                    >
                      <Trash2 className="h-4 w-4" />
                    </button>
                  </div>
                </div>
              </li>
            ))}
          </ul>
        </div>
      )}

      {showModal && (
        <div className="fixed z-10 inset-0 overflow-y-auto animate-fade-in">
          <div className="flex items-end justify-center min-h-screen pt-4 px-4 pb-20 text-center sm:block sm:p-0">
            <div className="fixed inset-0 bg-black/70 backdrop-blur-sm transition-opacity" onClick={() => setShowModal(false)}></div>

            <div className="inline-block align-bottom bg-[#202123] rounded-lg text-left overflow-hidden shadow-2xl transform transition-all sm:my-8 sm:align-middle sm:max-w-lg sm:w-full border border-white/10 animate-slide-up">
              <form onSubmit={handleSubmit}>
                <div className="px-4 pt-5 pb-4 sm:p-6 sm:pb-4">
                  <h3 className="text-lg leading-6 font-medium text-white mb-4">
                    {editingItem ? 'Edit Item' : 'Create Item'}
                  </h3>
                  <div className="space-y-4">
                    <div>
                      <label htmlFor="name" className="block text-sm font-medium text-gray-300">
                        Name
                      </label>
                      <input
                        type="text"
                        id="name"
                        required
                        value={formData.name}
                        onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                        className="mt-1 block w-full bg-slate-800/50 border border-slate-600 rounded-lg shadow-sm py-2 px-3 text-gray-100 placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-transparent transition-all duration-300"
                      />
                    </div>
                    <div>
                      <label htmlFor="description" className="block text-sm font-medium text-gray-300">
                        Description
                      </label>
                      <textarea
                        id="description"
                        required
                        rows={3}
                        value={formData.description}
                        onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                        className="mt-1 block w-full bg-slate-800/50 border border-slate-600 rounded-lg shadow-sm py-2 px-3 text-gray-100 placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-transparent transition-all duration-300"
                      />
                    </div>
                  </div>
                </div>
                <div className="bg-[#2a2b32] px-4 py-3 sm:px-6 sm:flex sm:flex-row-reverse border-t border-white/10">
                  <button
                    type="submit"
                    disabled={submitting}
                    className="w-full inline-flex justify-center rounded-xl border-0 shadow-lg px-4 py-2 gradient-purple text-base font-medium text-white hover:scale-105 hover-glow-purple focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-purple-500 sm:ml-3 sm:w-auto sm:text-sm disabled:opacity-50 disabled:hover:scale-100 transition-all duration-300"
                  >
                    {submitting ? (
                      <Loader2 className="h-5 w-5 animate-spin" />
                    ) : (
                      editingItem ? 'Update' : 'Create'
                    )}
                  </button>
                  <button
                    type="button"
                    onClick={() => {
                      setShowModal(false);
                      setEditingItem(null);
                      setFormData({ name: '', description: '' });
                    }}
                    className="mt-3 w-full inline-flex justify-center rounded-xl border-2 border-slate-600 shadow-sm px-4 py-2 bg-slate-800/50 text-base font-medium text-gray-300 hover:bg-slate-700/50 hover:scale-105 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-purple-500 sm:mt-0 sm:ml-3 sm:w-auto sm:text-sm transition-all duration-300"
                  >
                    Cancel
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
