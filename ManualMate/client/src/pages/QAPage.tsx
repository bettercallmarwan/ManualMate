import { useState, useEffect, useRef } from 'react';
import { itemApi } from '../services/api';
import type { Item } from '../types';
import { Send, Loader2, Bot, User } from 'lucide-react';

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
  const messagesEndRef = useRef<HTMLDivElement>(null);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  useEffect(() => {
    loadItems();
  }, []);

  useEffect(() => {
    scrollToBottom();
  }, [messages, asking]);

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
    <div className="fixed inset-0 top-16 flex flex-col bg-[#1a1b1e]">
      {/* Top Bar with Product Selector - Sticky */}
      <div className="flex-shrink-0 bg-[#202123] border-b border-white/10 px-4 py-3">
        <div className="max-w-3xl mx-auto flex items-center justify-between">
          <h1 className="text-lg font-semibold text-gray-100">Q&A Assistant</h1>
          <div className="flex items-center gap-3">
            <select
              value={selectedItem?.id || ''}
              onChange={(e) => {
                const item = items.find(p => p.id === parseInt(e.target.value));
                setSelectedItem(item || null);
                setMessages([]);
                setError(null);
              }}
              className="bg-[#40414f] border border-white/10 rounded-lg px-3 py-1.5 text-sm text-gray-100 focus:outline-none focus:ring-1 focus:ring-white/20"
              disabled={loading}
            >
              {items.map((item) => (
                <option key={item.id} value={item.id}>
                  {item.name} {item.filePath ? '✓' : ''}
                </option>
              ))}
            </select>
            {messages.length > 0 && (
              <button
                onClick={clearChat}
                className="text-sm text-gray-400 hover:text-gray-200 transition-colors px-3 py-1.5 rounded-lg hover:bg-white/5"
              >
                Clear
              </button>
            )}
          </div>
        </div>
      </div>

      {/* Messages Container - Scrollable */}
      <div className="flex-1 overflow-y-auto">
        {messages.length === 0 ? (
          <div className="flex items-center justify-center h-full">
            <div className="text-center">
              <Bot className="h-12 w-12 mx-auto mb-4 text-gray-600" />
              <p className="text-lg text-gray-400">How can I help you today?</p>
              <p className="text-sm mt-2 text-gray-500">Ask questions about your selected item's manual</p>
            </div>
          </div>
        ) : (
          <>
            {messages.map((message, index) => (
              <div
                key={index}
                className={`border-b border-white/5 ${message.role === 'user' ? 'bg-[#1a1b1e]' : 'bg-[#2a2b32]'
                  }`}
              >
                <div className="max-w-3xl mx-auto px-4 py-6">
                  <div className="flex gap-4">
                    <div className="flex-shrink-0">
                      {message.role === 'assistant' ? (
                        <div className="w-8 h-8 rounded-sm bg-[#10a37f] flex items-center justify-center">
                          <Bot className="h-5 w-5 text-white" />
                        </div>
                      ) : (
                        <div className="w-8 h-8 rounded-sm bg-[#5e5e70] flex items-center justify-center">
                          <User className="h-5 w-5 text-white" />
                        </div>
                      )}
                    </div>
                    <div className="flex-1 space-y-2">
                      <p className="text-gray-100 whitespace-pre-wrap leading-relaxed">{message.content}</p>
                      <p className="text-xs text-gray-500">{message.timestamp.toLocaleTimeString()}</p>
                    </div>
                  </div>
                </div>
              </div>
            ))}

            {asking && (
              <div className="bg-[#2a2b32] border-b border-white/5">
                <div className="max-w-3xl mx-auto px-4 py-6">
                  <div className="flex gap-4">
                    <div className="flex-shrink-0">
                      <div className="w-8 h-8 rounded-sm bg-[#10a37f] flex items-center justify-center">
                        <Bot className="h-5 w-5 text-white" />
                      </div>
                    </div>
                    <div className="flex items-center gap-2">
                      <Loader2 className="h-4 w-4 animate-spin text-gray-400" />
                      <span className="text-gray-400 text-sm">Thinking...</span>
                    </div>
                  </div>
                </div>
              </div>
            )}
            <div ref={messagesEndRef} />
          </>
        )}
      </div>

      {/* Error Message */}
      {error && (
        <div className="flex-shrink-0 bg-red-500/10 border-t border-red-500/20 px-4 py-2">
          <p className="text-sm text-red-400 max-w-3xl mx-auto">{error}</p>
        </div>
      )}

      {/* Input Form - Fixed at Bottom */}
      <div className="flex-shrink-0 border-t border-white/10 bg-[#202123]">
        <form onSubmit={handleAsk} className="max-w-3xl mx-auto px-4 py-4">
          <div className="flex items-end gap-3">
            <div className="flex-1 relative">
              <textarea
                value={question}
                onChange={(e) => setQuestion(e.target.value)}
                onKeyDown={(e) => {
                  if (e.key === 'Enter' && !e.shiftKey) {
                    e.preventDefault();
                    handleAsk(e);
                  }
                }}
                placeholder="Send a message..."
                rows={1}
                className="w-full bg-[#40414f] border border-white/10 rounded-lg px-4 py-3 text-gray-100 placeholder-gray-500 focus:outline-none focus:ring-1 focus:ring-white/20 resize-none max-h-32"
                disabled={asking || !selectedItem || !selectedItem?.filePath}
                style={{ minHeight: '52px' }}
              />
            </div>
            <button
              type="submit"
              disabled={asking || !question.trim() || !selectedItem || !selectedItem?.filePath}
              className="p-3 rounded-lg bg-[#10a37f] text-white hover:bg-[#0e8c6f] disabled:opacity-40 disabled:cursor-not-allowed transition-colors flex-shrink-0"
            >
              {asking ? (
                <Loader2 className="h-5 w-5 animate-spin" />
              ) : (
                <Send className="h-5 w-5" />
              )}
            </button>
          </div>
          {selectedItem && !selectedItem.filePath && (
            <p className="mt-2 text-xs text-red-400">
              ⚠️ This item has no manual. Please upload and process a manual first.
            </p>
          )}
        </form>
      </div>
    </div>
  );
}
