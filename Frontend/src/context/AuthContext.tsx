import { createContext, useCallback, useContext, useMemo, useState, type ReactNode } from 'react';
import { msmsApi } from '../api/msmsApi';
import type { Role } from '../types';

interface AuthState {
  roles: Role[];
  isAuthenticated: boolean;
  login: (email: string, password: string) => Promise<Role[]>;
  logout: () => void;
  hasRole: (role: Role) => boolean;
}

const AuthContext = createContext<AuthState | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [roles, setRoles] = useState<Role[]>(() => {
    const raw = localStorage.getItem('msms_roles');
    return raw ? (JSON.parse(raw) as Role[]) : [];
  });

  const login = useCallback(async (email: string, password: string) => {
    const { data } = await msmsApi.auth.login(email, password);
    localStorage.setItem('msms_access_token', data.accessToken);
    localStorage.setItem('msms_refresh_token', data.refreshToken);
    const userRoles = data.roles as Role[];
    localStorage.setItem('msms_roles', JSON.stringify(userRoles));
    setRoles(userRoles);
    return userRoles;
  }, []);

  const logout = useCallback(() => {
    localStorage.removeItem('msms_access_token');
    localStorage.removeItem('msms_refresh_token');
    localStorage.removeItem('msms_roles');
    setRoles([]);
  }, []);

  const value = useMemo(
    () => ({
      roles,
      isAuthenticated: roles.length > 0,
      login,
      logout,
      hasRole: (role: Role) => roles.includes(role),
    }),
    [roles, login, logout]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
}

export function getHomePath(roles: Role[]): string {
  if (roles.includes('Admin')) return '/admin';
  if (roles.includes('Teacher')) return '/teacher';
  if (roles.includes('Parent')) return '/parent';
  if (roles.includes('Student')) return '/student';
  return '/login';
}
