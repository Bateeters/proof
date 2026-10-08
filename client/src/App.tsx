import { Routes, Route, Navigate } from 'react-router-dom'
import { Layout } from './components/Layout'
import { ProtectedRoute } from './components/ProtectedRoute'
import { RequireProfile } from './components/RequireProfile'
import { AuthPage } from './pages/AuthPage'
import { WhoIsDrinkingPage } from './pages/WhoIsDrinkingPage'
import { Home } from './pages/Home'
import { CategoryPage } from './pages/CategoryPage'
import { CocktailDetailPage } from './pages/CocktailDetailPage'
import { CocktailFormPage } from './pages/CocktailFormPage'
import { MyCreationsPage } from './pages/MyCreationsPage'
import { AccountSettingsPage } from './pages/AccountSettingsPage'
import { PreferencesEditor } from './components/PreferencesEditor'
import { Recommendations } from './components/Recommendations'
import { Cookbook } from './components/Cookbook'
import { WhatCanIMake } from './components/WhatCanIMake'

function App() {
  return (
    <Routes>
      <Route path="/login" element={<AuthPage />} />

      <Route element={<ProtectedRoute />}>
        <Route path="/profiles" element={<WhoIsDrinkingPage />} />

        <Route element={<RequireProfile />}>
          <Route element={<Layout />}>
            <Route path="/" element={<Home />} />
            <Route path="/category/:categoryName" element={<CategoryPage />} />
            <Route path="/cocktails/new" element={<CocktailFormPage />} />
            <Route path="/cocktails/:cocktailId" element={<CocktailDetailPage />} />
            <Route path="/cocktails/:cocktailId/edit" element={<CocktailFormPage />} />
            <Route path="/my-creations" element={<MyCreationsPage />} />
            <Route path="/recommendations" element={<div className="max-w-3xl"><Recommendations /></div>} />
            <Route path="/cookbook" element={<Cookbook />} />
            <Route path="/what-can-i-make" element={<div className="max-w-3xl"><WhatCanIMake /></div>} />
            <Route path="/preferences" element={<div className="max-w-3xl"><PreferencesEditor /></div>} />
            <Route path="/account" element={<AccountSettingsPage />} />
          </Route>
        </Route>
      </Route>

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}

export default App
