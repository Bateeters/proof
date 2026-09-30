import { Routes, Route, Navigate } from 'react-router-dom'
import { Layout } from './components/Layout'
import { ProtectedRoute } from './components/ProtectedRoute'
import { AuthPage } from './pages/AuthPage'
import { Home } from './pages/Home'
import { CategoryPage } from './pages/CategoryPage'
import { CocktailDetailPage } from './pages/CocktailDetailPage'
import { PreferencesEditor } from './components/PreferencesEditor'
import { Recommendations } from './components/Recommendations'
import { Cookbook } from './components/Cookbook'
import { WhatCanIMake } from './components/WhatCanIMake'

function App() {
  return (
    <Routes>
      <Route path="/login" element={<AuthPage />} />

      <Route element={<ProtectedRoute />}>
        <Route element={<Layout />}>
          <Route path="/" element={<Home />} />
          <Route path="/category/:categoryName" element={<CategoryPage />} />
          <Route path="/cocktails/:cocktailId" element={<CocktailDetailPage />} />
          <Route path="/recommendations" element={<div className="max-w-3xl"><Recommendations /></div>} />
          <Route path="/cookbook" element={<div className="max-w-3xl"><Cookbook /></div>} />
          <Route path="/what-can-i-make" element={<div className="max-w-3xl"><WhatCanIMake /></div>} />
          <Route path="/preferences" element={<div className="max-w-3xl"><PreferencesEditor /></div>} />
        </Route>
      </Route>

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}

export default App
