import { BrowserRouter as Router, Routes, Route, Link } from 'react-router-dom';
import ItemsPage from './pages/ItemsPage';
import ManualPage from './pages/ManualPage';
import QAPage from './pages/QAPage';
import { BookOpen, Package, MessageCircle } from 'lucide-react';

function App() {
  return (
    <Router>
      <div className="min-h-screen bg-[#1a1b1e]">
        <nav className="bg-[#202123] sticky top-0 z-50 border-b border-white/10">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
            <div className="flex justify-between h-16">
              <div className="flex">
                <div className="flex-shrink-0 flex items-center">
                  <BookOpen className="h-8 w-8 text-gray-100" />
                  <span className="ml-2 text-xl font-bold text-gray-100">ManualMate</span>
                </div>
                <div className="hidden sm:ml-6 sm:flex sm:space-x-8">
                  <Link
                    to="/"
                    className="inline-flex items-center px-1 pt-1 text-sm font-medium text-gray-300 hover:text-white border-b-2 border-transparent hover:border-gray-400 transition-all duration-200"
                  >
                    <Package className="h-4 w-4 mr-2" />
                    Items
                  </Link>
                  <Link
                    to="/manual"
                    className="inline-flex items-center px-1 pt-1 text-sm font-medium text-gray-300 hover:text-white border-b-2 border-transparent hover:border-gray-400 transition-all duration-200"
                  >
                    <BookOpen className="h-4 w-4 mr-2" />
                    Files
                  </Link>
                  <Link
                    to="/qa"
                    className="inline-flex items-center px-1 pt-1 text-sm font-medium text-gray-300 hover:text-white border-b-2 border-transparent hover:border-gray-400 transition-all duration-200"
                  >
                    <MessageCircle className="h-4 w-4 mr-2" />
                    Q&A
                  </Link>
                </div>
              </div>
            </div>
          </div>
        </nav>

        <main className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
          <Routes>
            <Route path="/" element={<ItemsPage />} />
            <Route path="/manual" element={<ManualPage />} />
            <Route path="/qa" element={<QAPage />} />
          </Routes>
        </main>
      </div>
    </Router>
  );
}

export default App;
