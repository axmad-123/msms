import { useEffect, useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { School, Shield, Users, GraduationCap, ArrowRight, Sparkles } from 'lucide-react';
import { getErrorMessage } from '../api/client';
import { useAuth, getHomePath } from '../context/AuthContext';
import { Alert, Button, Input } from '../components/ui';

export function LoginPage() {
  const { login, isAuthenticated, roles } = useAuth();
  const navigate = useNavigate();
  const [email, setEmail] = useState('admin@msms.local');
  const [password, setPassword] = useState('ChangeMe!12345');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (isAuthenticated) navigate(getHomePath(roles), { replace: true });
  }, [isAuthenticated, roles, navigate]);

  const submit = async (e: FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      const userRoles = await login(email, password);
      navigate(getHomePath(userRoles), { replace: true });
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  const features = [
    { icon: Users, text: 'Student & parent portals' },
    { icon: GraduationCap, text: 'Exams & attendance tracking' },
    { icon: Shield, text: 'Secure role-based access' },
  ];

  return (
    <div className="flex min-h-screen">
      {/* Brand panel */}
      <aside className="relative hidden w-[52%] overflow-hidden lg:flex lg:flex-col lg:justify-between">
        <div className="absolute inset-0 bg-gradient-to-br from-slate-900 via-indigo-950 to-slate-900" />
        <div className="absolute inset-0 bg-[url('data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNjAiIGhlaWdodD0iNjAiIHZpZXdCb3g9IjAgMCA2MCA2MCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48ZyBmaWxsPSJub25lIiBmaWxsLXJ1bGU9ImV2ZW5vZGQiPjxnIGZpbGw9IiNmZmYiIGZpbGwtb3BhY2l0eT0iMC4wMyI+PHBhdGggZD0iTTM2IDM0djItSDI0di0yaDEyek0zNiAyNHYySDI0VjI0aDEyeiIvPjwvZz48L2c+PC9zdmc+')] opacity-40" />
        <div className="absolute -left-20 top-20 h-72 w-72 rounded-full bg-indigo-500/20 blur-3xl" />
        <div className="absolute -right-10 bottom-20 h-96 w-96 rounded-full bg-sky-500/15 blur-3xl" />

        <div className="relative z-10 p-12">
          <div className="flex items-center gap-3">
            <span className="flex h-12 w-12 items-center justify-center rounded-2xl bg-gradient-to-br from-indigo-500 to-violet-600 shadow-xl shadow-indigo-500/40">
              <School className="h-6 w-6 text-white" />
            </span>
            <span className="text-2xl font-bold text-white">MSMS</span>
          </div>
        </div>

        <div className="relative z-10 px-12">
          <span className="mb-4 inline-flex items-center gap-2 rounded-full border border-white/10 bg-white/5 px-4 py-1.5 text-xs font-semibold text-indigo-200 backdrop-blur">
            <Sparkles className="h-3.5 w-3.5" />
            Modern School Management
          </span>
          <h1 className="max-w-lg text-4xl font-bold leading-tight tracking-tight text-white xl:text-5xl">
            Run your school with clarity and confidence
          </h1>
          <p className="mt-5 max-w-md text-base leading-relaxed text-slate-300">
            One platform for students, teachers, parents, and administrators — attendance, exams, finance, and more.
          </p>
          <ul className="mt-10 space-y-4">
            {features.map((f) => (
              <li key={f.text} className="flex items-center gap-3 text-sm text-slate-300">
                <span className="flex h-9 w-9 items-center justify-center rounded-xl bg-white/10">
                  <f.icon className="h-4 w-4 text-indigo-300" />
                </span>
                {f.text}
              </li>
            ))}
          </ul>
        </div>

        <p className="relative z-10 p-12 text-xs text-slate-500">© 2026 MSMS · All rights reserved</p>
      </aside>

      {/* Login form */}
      <main className="flex flex-1 flex-col items-center justify-center bg-gradient-to-br from-slate-50 to-indigo-50/30 p-6 sm:p-10">
        <div className="mb-8 flex items-center gap-3 lg:hidden">
          <span className="flex h-11 w-11 items-center justify-center rounded-2xl bg-gradient-to-br from-indigo-600 to-violet-600 text-white">
            <School className="h-5 w-5" />
          </span>
          <span className="text-xl font-bold text-slate-900">MSMS</span>
        </div>

        <div className="w-full max-w-md animate-fade-in">
          <div className="rounded-3xl border border-slate-200/80 bg-white p-8 shadow-[0_8px_40px_rgba(15,23,42,0.08)] sm:p-10">
            <h2 className="text-2xl font-bold tracking-tight text-slate-900">Welcome back</h2>
            <p className="mt-2 text-sm text-slate-500">Sign in to your account to continue</p>

            {error && (
              <div className="mt-6">
                <Alert message={error} />
              </div>
            )}

            <form onSubmit={submit} className="mt-8 space-y-5">
              <Input
                label="Email address"
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="you@school.edu"
                required
              />
              <Input
                label="Password"
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="••••••••"
                required
              />
              <Button type="submit" className="w-full" size="lg" loading={loading}>
                Sign in
                <ArrowRight className="h-4 w-4" />
              </Button>
            </form>

            <div className="mt-8 rounded-2xl border border-slate-100 bg-slate-50/80 p-4">
              <p className="text-xs font-semibold uppercase tracking-wider text-slate-400">Demo accounts</p>
              <p className="mt-2 text-xs leading-relaxed text-slate-600">
                <span className="font-medium text-slate-800">Admin:</span> admin@msms.local / ChangeMe!12345
                <br />
                <span className="font-medium text-slate-800">Others:</span> teacher1@, parent1@, student001@msms.local — Demo@12345
              </p>
            </div>
          </div>
        </div>
      </main>
    </div>
  );
}
