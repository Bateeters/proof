import { LoginForm } from './components/LoginForm'
import { RegisterForm } from './components/RegisterForm'
import { ProfileSwitcher } from './components/ProfileSwitcher'
import { CocktailDiscovery } from './components/CocktailDiscovery'
import { PreferencesEditor } from './components/PreferencesEditor'
import { Recommendations } from './components/Recommendations'
import { Cookbook } from './components/Cookbook'
import { WhatCanIMake } from './components/WhatCanIMake'
import { useAuth } from './context/AuthContext'
import { useProfiles } from './context/ProfileContext'
import './App.css'

function App() {
  const { token, account, logout } = useAuth();
  const { activeProfile } = useProfiles();

  return (
    <div className="app-shell">
      <header className="app-header">
        <h1>Proof</h1>
        {token && (
          <div className="app-header-status">
            <span>{account?.email}</span>
            <span className="app-header-profile">
              {activeProfile ? activeProfile.displayName : 'No profile selected'}
            </span>
            <button className="button-secondary" onClick={logout}>Log Out</button>
          </div>
        )}
      </header>

      {token ? (
        <main className="app-main">
          <section className="app-section" aria-label="Profiles">
            <ProfileSwitcher />
          </section>

          <section className="app-section">
            <h2>Discover Cocktails</h2>
            <CocktailDiscovery />
          </section>

          <section className="app-section">
            <h2>Taste Preferences</h2>
            <PreferencesEditor />
          </section>

          <section className="app-section">
            <h2>Recommended For You</h2>
            <Recommendations />
          </section>

          <section className="app-section">
            <h2>Your Cookbook</h2>
            <Cookbook />
          </section>

          <section className="app-section">
            <WhatCanIMake />
          </section>
        </main>
      ) : (
        <main className="app-main app-auth">
          <section className="app-section">
            <h2>Log In</h2>
            <LoginForm />
          </section>
          <section className="app-section">
            <h2>Register</h2>
            <RegisterForm />
          </section>
        </main>
      )}
    </div>
  )
}

export default App
