import { useState, useEffect } from 'react';
import { itemApi } from '../services/api';
import type { Item } from '../types';
import { Send, Loader2, MessageCircle, Bot, User } from 'lucide-react';

interface Message {
  role: 'user' | 'assistant';
  content: string;
  timestamp: Date;
}

export default function QAPage() {
  const [items, setItems] = useState<Item[]>([]);
  const [selectedItem, setSelectedItem] = useState<Item | null>(null);
  const [question, setQuestion] = useState('');
  const [messages, setMessages] = useState<Message[]>([]);
  const [loading, setLoading] = useState(true);
  const [asking, setAsking] = useState(false);
  const [error, setError] = useState<string | null>(null);

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
      setError(err.response?.data?.error || 'Failed to load items');
    } finally {
      setLoading(false);
    }
  };

  const handleAsk = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!selectedItem || !question.trim()) {
      setError('Please select an item and enter a question');
      return;
    }

    if (!selectedItem.filePath) {
      setError('This item has no manual. Please upload and process a manual first.');
      return;
    }

    const userMessage: Message = {
      role: 'user',
      content: question,
      timestamp: new Date(),
    };

    setMessages(prev => [...prev, userMessage]);
    setAsking(true);
    setError(null);
    const currentQuestion = question;
    setQuestion('');

    try {
      const answer = await itemApi.ask(selectedItem.id, currentQuestion);
      const assistantMessage: Message = {
        role: 'assistant',
        content: answer,
        timestamp: new Date(),
      };
      setMessages(prev => [...prev, assistantMessage]);
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to get answer');
      setMessages(prev => prev.slice(0, -1)); // Remove user message if error
    } finally {
      setAsking(false);
    }
  };

  const clearChat = () => {
    setMessages([]);
    setError(null);
  };

  return (
    <div className="px-4 py-6 sm:px-0 h-[calc(100vh-8rem)] flex flex-col">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold text-gray-900 flex items-center">
          <MessageCircle className="h-8 w-8 mr-2 text-primary-600" />
          Q&A Assistant
        </h1>
        {messages.length > 0 && (
          <button
            onClick={clearChat}
            className="text-sm text-gray-600 hover:text-gray-900"
          >
            Clear Chat
          </button>
        )}
      </div>

      <div className="bg-white shadow rounded-lg flex-1 flex flex-col overflow-hidden">
        {/* Product Selection */}
        <div className="border-b border-gray-200 p-4">
          <label htmlFor="qa-product-select" className="block text-sm font-medium text-gray-700 mb-2">
            Select Product
          </label>
          <select
            id="qa-product-select"
            value={selectedItem?.id || ''}
            onChange={(e) => {
              const item = items.find(p => p.id === parseInt(e.target.value));
              setSelectedItem(item || null);
              setMessages([]);
              setError(null);
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
          {selectedItem && !selectedItem.filePath && (
            <p className="mt-2 text-sm text-red-600">
              ⚠️ This item has no manual. Please upload and process a manual first.
            </p>
          )}
        </div>

        {/* Messages */}
        <div className="flex-1 overflow-y-auto p-4 space-y-4">
          {messages.length === 0 ? (
            <div className="flex items-center justify-center h-full text-gray-400">
              <div className="text-center">
                <Bot className="h-12 w-12 mx-auto mb-4" />
                <p className="text-lg">Ask a question about the item manual</p>
                <p className="text-sm mt-2">Select an item and start asking questions!</p>
              </div>
            </div>
          ) : (
            messages.map((message, index) => (
              <div
                key={index}
                className={`flex ${message.role === 'user' ? 'justify-end' : 'justify-start'}`}
              >
                <div
                  className={`max-w-3xl rounded-lg px-4 py-2 ${message.role === 'user'
                    ? 'bg-primary-600 text-white'
                    : 'bg-gray-100 text-gray-900'
                    }`}
                >
                  <div className="flex items-start">
                    {message.role === 'assistant' && (
                      <Bot className="h-5 w-5 mr-2 mt-0.5 flex-shrink-0" />
                    )}
                    {message.role === 'user' && (
                      <User className="h-5 w-5 mr-2 mt-0.5 flex-shrink-0" />
                    )}
                    <div className="flex-1">
                      <p className="whitespace-pre-wrap">{message.content}</p>
                      <p className={`text-xs mt-1 ${message.role === 'user' ? 'text-primary-100' : 'text-gray-500'
                        }`}>
                        {message.timestamp.toLocaleTimeString()}
                      </p>
                    </div>
                  </div>
                </div>
              </div>
            ))
          )}

          {asking && (
            <div className="flex justify-start">
              <div className="bg-gray-100 rounded-lg px-4 py-2">
                <div className="flex items-center">
                  <Bot className="h-5 w-5 mr-2" />
                  <Loader2 className="h-5 w-5 animate-spin text-primary-600" />
                  <span className="ml-2 text-gray-600">Thinking...</span>
                </div>
              </div>
            </div>
          )}
        </div>

        {/* Error Message */}
        {error && (
          <div className="px-4 py-2 bg-red-50 border-t border-red-200">
            <p className="text-sm text-red-800">{error}</p>
          </div>
        )}

        {/* Input Form */}
        <form onSubmit={handleAsk} className="border-t border-gray-200 p-4">
          <div className="flex space-x-2">
            <input
              type="text"
              value={question}
              onChange={(e) => setQuestion(e.target.value)}
              placeholder="Ask a question about the item manual..."
              className="flex-1 border border-gray-300 rounded-md shadow-sm py-2 px-4 focus:outline-none focus:ring-primary-500 focus:border-primary-500"
              disabled={asking || !selectedItem || !selectedItem?.filePath}
            />
            <button
              type="submit"
              disabled={asking || !question.trim() || !selectedItem || !selectedItem?.filePath}
              className="inline-flex items-center px-6 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-primary-600 hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {asking ? (
                <Loader2 className="h-5 w-5 animate-spin" />
              ) : (
                <Send className="h-5 w-5" />
              )}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
