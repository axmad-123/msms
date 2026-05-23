import type { ButtonHTMLAttributes, InputHTMLAttributes, ReactNode, SelectHTMLAttributes } from 'react';
import { Loader2, X, type LucideIcon } from 'lucide-react';

/* ─── Layout primitives ─── */

export function Card({ children, className = '', hover = false }: { children: ReactNode; className?: string; hover?: boolean }) {
  return (
    <div
      className={`rounded-2xl border border-slate-200/80 bg-white shadow-[0_1px_3px_rgba(15,23,42,0.06),0_4px_12px_rgba(15,23,42,0.04)] ${hover ? 'transition hover:border-indigo-200 hover:shadow-md' : ''} ${className}`}
    >
      {children}
    </div>
  );
}

export function CardHeader({ title, subtitle, action }: { title: string; subtitle?: string; action?: ReactNode }) {
  return (
    <div className="flex flex-wrap items-start justify-between gap-3 border-b border-slate-100/80 px-6 py-5">
      <div>
        <h2 className="text-base font-semibold tracking-tight text-slate-900">{title}</h2>
        {subtitle && <p className="mt-1 text-sm text-slate-500">{subtitle}</p>}
      </div>
      {action}
    </div>
  );
}

export function CardBody({ children, className = '' }: { children: ReactNode; className?: string }) {
  return <div className={`px-6 py-5 ${className}`}>{children}</div>;
}

/* ─── Stat card ─── */

const statThemes = {
  indigo: { bg: 'bg-indigo-500', glow: 'bg-indigo-500', light: 'bg-indigo-50 text-indigo-600' },
  sky: { bg: 'bg-sky-500', glow: 'bg-sky-500', light: 'bg-sky-50 text-sky-600' },
  violet: { bg: 'bg-violet-500', glow: 'bg-violet-500', light: 'bg-violet-50 text-violet-600' },
  emerald: { bg: 'bg-emerald-500', glow: 'bg-emerald-500', light: 'bg-emerald-50 text-emerald-600' },
  amber: { bg: 'bg-amber-500', glow: 'bg-amber-500', light: 'bg-amber-50 text-amber-600' },
  rose: { bg: 'bg-rose-500', glow: 'bg-rose-500', light: 'bg-rose-50 text-rose-600' },
} as const;

export function StatCard({
  label,
  value,
  icon: Icon,
  theme = 'indigo',
  trend,
}: {
  label: string;
  value: string | number;
  icon: LucideIcon;
  theme?: keyof typeof statThemes;
  trend?: string;
}) {
  const t = statThemes[theme];
  return (
    <Card hover className="stat-card-glow overflow-hidden">
      <CardBody className="relative">
        <span className={`absolute -right-4 -top-4 h-24 w-24 rounded-full ${t.glow} opacity-[0.08]`} />
        <div className="relative flex items-start justify-between">
          <div>
            <p className="text-sm font-medium text-slate-500">{label}</p>
            <p className="mt-2 text-3xl font-bold tracking-tight text-slate-900">{value}</p>
            {trend && <p className="mt-1.5 text-xs font-medium text-emerald-600">{trend}</p>}
          </div>
          <span className={`flex h-12 w-12 shrink-0 items-center justify-center rounded-2xl ${t.light}`}>
            <Icon className="h-6 w-6" strokeWidth={1.75} />
          </span>
        </div>
      </CardBody>
    </Card>
  );
}

/* ─── Tabs ─── */

export function Tabs<T extends string>({
  tabs,
  active,
  onChange,
}: {
  tabs: { id: T; label: string }[];
  active: T;
  onChange: (id: T) => void;
}) {
  return (
    <div className="inline-flex rounded-xl border border-slate-200/80 bg-slate-100/60 p-1">
      {tabs.map((tab) => (
        <button
          key={tab.id}
          type="button"
          onClick={() => onChange(tab.id)}
          className={`rounded-lg px-4 py-2 text-sm font-medium transition-all ${
            active === tab.id
              ? 'bg-white text-indigo-700 shadow-sm'
              : 'text-slate-600 hover:text-slate-900'
          }`}
        >
          {tab.label}
        </button>
      ))}
    </div>
  );
}

/* ─── Buttons & inputs ─── */

export function Button({
  variant = 'primary',
  size = 'md',
  loading,
  children,
  className = '',
  ...props
}: ButtonHTMLAttributes<HTMLButtonElement> & {
  variant?: 'primary' | 'secondary' | 'danger' | 'ghost';
  size?: 'sm' | 'md' | 'lg';
  loading?: boolean;
}) {
  const base =
    'inline-flex items-center justify-center gap-2 font-semibold transition-all focus:outline-none focus:ring-2 focus:ring-offset-2 disabled:pointer-events-none disabled:opacity-50';
  const sizes = { sm: 'rounded-lg px-3 py-1.5 text-xs', md: 'rounded-xl px-4 py-2.5 text-sm', lg: 'rounded-xl px-5 py-3 text-sm' };
  const variants = {
    primary:
      'bg-gradient-to-r from-indigo-600 to-indigo-500 text-white shadow-md shadow-indigo-500/25 hover:from-indigo-700 hover:to-indigo-600 focus:ring-indigo-500',
    secondary:
      'border border-slate-200 bg-white text-slate-700 shadow-sm hover:border-slate-300 hover:bg-slate-50 focus:ring-slate-300',
    danger: 'bg-gradient-to-r from-red-600 to-rose-600 text-white shadow-md shadow-red-500/20 hover:from-red-700 focus:ring-red-500',
    ghost: 'text-slate-600 hover:bg-slate-100 focus:ring-slate-300',
  };
  return (
    <button className={`${base} ${sizes[size]} ${variants[variant]} ${className}`} disabled={loading || props.disabled} {...props}>
      {loading && <Loader2 className="h-4 w-4 animate-spin" />}
      {children}
    </button>
  );
}

export function Input({
  label,
  error,
  className = '',
  ...props
}: InputHTMLAttributes<HTMLInputElement> & { label?: string; error?: string }) {
  return (
    <label className="block">
      {label && <span className="mb-2 block text-sm font-medium text-slate-700">{label}</span>}
      <input
        className={`w-full rounded-xl border border-slate-200 bg-slate-50/50 px-4 py-2.5 text-sm text-slate-900 outline-none transition placeholder:text-slate-400 focus:border-indigo-500 focus:bg-white focus:ring-4 focus:ring-indigo-500/10 ${className}`}
        {...props}
      />
      {error && <span className="mt-1.5 block text-xs font-medium text-red-600">{error}</span>}
    </label>
  );
}

export function Select({
  label,
  children,
  className = '',
  ...props
}: SelectHTMLAttributes<HTMLSelectElement> & { label?: string }) {
  return (
    <label className="block">
      {label && <span className="mb-2 block text-sm font-medium text-slate-700">{label}</span>}
      <select
        className={`w-full rounded-xl border border-slate-200 bg-slate-50/50 px-4 py-2.5 text-sm text-slate-900 outline-none transition focus:border-indigo-500 focus:bg-white focus:ring-4 focus:ring-indigo-500/10 ${className}`}
        {...props}
      >
        {children}
      </select>
    </label>
  );
}

/* ─── Feedback ─── */

export function Badge({ children, tone = 'default' }: { children: ReactNode; tone?: 'default' | 'success' | 'warning' | 'danger' | 'info' }) {
  const tones = {
    default: 'bg-slate-100 text-slate-700 ring-slate-200/60',
    success: 'bg-emerald-50 text-emerald-700 ring-emerald-200/60',
    warning: 'bg-amber-50 text-amber-800 ring-amber-200/60',
    danger: 'bg-red-50 text-red-700 ring-red-200/60',
    info: 'bg-indigo-50 text-indigo-700 ring-indigo-200/60',
  };
  return (
    <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-semibold ring-1 ring-inset ${tones[tone]}`}>
      {children}
    </span>
  );
}

export function Alert({ message, type = 'error' }: { message: string; type?: 'error' | 'success' | 'info' }) {
  const styles = {
    error: 'border-red-200/80 bg-red-50 text-red-800',
    success: 'border-emerald-200/80 bg-emerald-50 text-emerald-800',
    info: 'border-indigo-200/80 bg-indigo-50 text-indigo-800',
  };
  return (
    <div className={`rounded-xl border px-4 py-3 text-sm font-medium ${styles[type]}`}>{message}</div>
  );
}

export function Modal({ open, onClose, title, children, wide }: { open: boolean; onClose: () => void; title: string; children: ReactNode; wide?: boolean }) {
  if (!open) return null;
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div className="absolute inset-0 bg-slate-900/60 backdrop-blur-sm" onClick={onClose} aria-hidden />
      <div
        className={`relative z-10 w-full animate-fade-in rounded-2xl border border-slate-200/80 bg-white shadow-2xl ${wide ? 'max-w-2xl' : 'max-w-lg'}`}
        role="dialog"
      >
        <div className="flex items-center justify-between border-b border-slate-100 px-6 py-4">
          <h3 className="text-lg font-semibold text-slate-900">{title}</h3>
          <button
            type="button"
            onClick={onClose}
            className="rounded-lg p-2 text-slate-400 transition hover:bg-slate-100 hover:text-slate-600"
          >
            <X className="h-5 w-5" />
          </button>
        </div>
        <div className="max-h-[70vh] overflow-y-auto px-6 py-5">{children}</div>
      </div>
    </div>
  );
}

/* ─── Page chrome ─── */

export function PageHeader({ title, description, action, badge }: { title: string; description?: string; action?: ReactNode; badge?: string }) {
  return (
    <header className="mb-8 animate-fade-in">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          {badge && (
            <span className="mb-2 inline-flex rounded-full bg-indigo-50 px-3 py-1 text-xs font-semibold uppercase tracking-wider text-indigo-600">
              {badge}
            </span>
          )}
          <h1 className="text-2xl font-bold tracking-tight text-slate-900 sm:text-3xl">{title}</h1>
          {description && <p className="mt-2 max-w-2xl text-sm leading-relaxed text-slate-500">{description}</p>}
        </div>
        {action && <div className="flex shrink-0 flex-wrap items-center gap-2">{action}</div>}
      </div>
    </header>
  );
}

export function EmptyState({ message, icon: Icon }: { message: string; icon?: LucideIcon }) {
  return (
    <div className="flex flex-col items-center justify-center py-16 text-center">
      {Icon && (
        <span className="mb-4 flex h-14 w-14 items-center justify-center rounded-2xl bg-slate-100 text-slate-400">
          <Icon className="h-7 w-7" />
        </span>
      )}
      <p className="text-sm font-medium text-slate-500">{message}</p>
    </div>
  );
}

/* ─── Tables ─── */

export function TableWrap({ children }: { children: ReactNode }) {
  return (
    <div className="overflow-x-auto">
      <table className="w-full min-w-[640px] text-left text-sm">{children}</table>
    </div>
  );
}

export function Th({ children }: { children: ReactNode }) {
  return (
    <th className="whitespace-nowrap border-b border-slate-200/80 bg-slate-50/80 px-5 py-3.5 text-xs font-semibold uppercase tracking-wider text-slate-500">
      {children}
    </th>
  );
}

export function Td({ children, className = '' }: { children: ReactNode; className?: string }) {
  return (
    <td className={`border-b border-slate-100/80 px-5 py-4 text-slate-700 ${className}`}>{children}</td>
  );
}

export function TableRow({ children, className = '' }: { children: ReactNode; className?: string }) {
  return <tr className={`transition hover:bg-indigo-50/30 ${className}`}>{children}</tr>;
}
