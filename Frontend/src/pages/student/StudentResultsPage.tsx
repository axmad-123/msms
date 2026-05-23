import { useEffect, useMemo, useState } from 'react';
import { Award, BookOpen, MapPin, User } from 'lucide-react';
import { msmsApi } from '../../api/msmsApi';
import type { ExamResult, StudentPortal } from '../../types';
import { EXAM_TYPES, GENDER_LABELS, SCHOOL_SECTIONS } from '../../types';
import { Card, CardBody, EmptyState, PageHeader } from '../../components/ui';

const EXAM_ORDER = [1, 2, 3, 4] as const;

const examThemes: Record<number, { gradient: string; accent: string; icon: string }> = {
  1: { gradient: 'from-sky-500 to-blue-600', accent: 'text-sky-600', icon: 'bg-sky-100' },
  2: { gradient: 'from-violet-500 to-purple-600', accent: 'text-violet-600', icon: 'bg-violet-100' },
  3: { gradient: 'from-amber-500 to-orange-600', accent: 'text-amber-600', icon: 'bg-amber-100' },
  4: { gradient: 'from-emerald-500 to-teal-600', accent: 'text-emerald-600', icon: 'bg-emerald-100' },
};

function groupByExam(results: ExamResult[]) {
  const map = new Map<number, ExamResult[]>();
  for (const t of EXAM_ORDER) map.set(t, []);
  for (const r of results) {
    const list = map.get(r.examType) ?? [];
    list.push(r);
    map.set(r.examType, list);
  }
  return map;
}

export function StudentResultsPage() {
  const [portal, setPortal] = useState<StudentPortal | null>(null);

  useEffect(() => {
    msmsApi.student.myPortal().then((r) => setPortal(r.data));
  }, []);

  const grouped = useMemo(
    () => groupByExam(portal?.examResults ?? []),
    [portal?.examResults]
  );

  if (!portal) {
    return (
      <>
        <PageHeader title="My Profile & Results" description="Loading your student card…" />
        <Card><CardBody><EmptyState message="Loading…" /></CardBody></Card>
      </>
    );
  }

  const p = portal.profile;

  return (
    <>
      <PageHeader title="My Profile & Results" description="Student ID and exam results by term" />

      {/* Student ID card */}
      <Card className="mb-8 overflow-hidden">
        <div className="bg-gradient-to-br from-slate-900 via-indigo-950 to-slate-900 px-6 py-8 text-white sm:px-10">
          <div className="flex flex-col gap-6 sm:flex-row sm:items-center sm:justify-between">
            <div className="flex items-center gap-5">
              {p.photoUrl ? (
                <img
                  src={p.photoUrl}
                  alt=""
                  className="h-24 w-24 rounded-2xl border-4 border-white/20 object-cover shadow-xl"
                />
              ) : (
                <span className="flex h-24 w-24 items-center justify-center rounded-2xl border-4 border-white/20 bg-white/10">
                  <User className="h-12 w-12 text-white/70" />
                </span>
              )}
              <div>
                <p className="text-xs font-semibold uppercase tracking-widest text-indigo-300">Student ID</p>
                <h2 className="mt-1 text-2xl font-bold tracking-tight">{p.firstName} {p.lastName}</h2>
                <p className="mt-1 font-mono text-lg text-indigo-200">{p.studentNumber}</p>
              </div>
            </div>
            <div className="rounded-xl bg-white/10 px-5 py-4 backdrop-blur-sm">
              <p className="text-xs uppercase tracking-wide text-slate-300">Class</p>
              <p className="text-lg font-semibold">{p.className ?? '—'}</p>
              <p className="text-sm text-indigo-200">Grade {p.gradeLevel ?? '—'}</p>
            </div>
          </div>
          <div className="mt-6 grid gap-4 border-t border-white/10 pt-6 sm:grid-cols-3">
            <div className="flex items-start gap-2 text-sm">
              <MapPin className="mt-0.5 h-4 w-4 shrink-0 text-indigo-300" />
              <div>
                <p className="text-slate-400">Place of birth</p>
                <p className="font-medium">{p.placeOfBirth ?? '—'}</p>
              </div>
            </div>
            <div>
              <p className="text-sm text-slate-400">Gender</p>
              <p className="font-medium">{p.gender != null ? GENDER_LABELS[p.gender] : '—'}</p>
            </div>
            <div>
              <p className="text-sm text-slate-400">Section</p>
              <p className="font-medium">
                {p.schoolSection != null ? SCHOOL_SECTIONS[p.schoolSection] : '—'}
              </p>
            </div>
          </div>
        </div>
      </Card>

      {/* 4 exam cards */}
      <h3 className="mb-4 text-lg font-semibold text-slate-900">Exam Results</h3>
      <div className="grid gap-6 md:grid-cols-2">
        {EXAM_ORDER.map((examType) => {
          const rows = grouped.get(examType) ?? [];
          const theme = examThemes[examType];
          const title = EXAM_TYPES[examType] ?? `Exam ${examType}`;
          const avg = rows.length
            ? rows.reduce((s, r) => s + (r.marks / r.maxMarks) * 100, 0) / rows.length
            : null;

          return (
            <Card key={examType} className="overflow-hidden">
              <div className={`bg-gradient-to-r ${theme.gradient} px-5 py-4 text-white`}>
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-3">
                    <span className={`flex h-10 w-10 items-center justify-center rounded-xl ${theme.icon}`}>
                      <BookOpen className={`h-5 w-5 ${theme.accent}`} />
                    </span>
                    <div>
                      <p className="text-xs font-medium uppercase tracking-wide text-white/80">Exam</p>
                      <h4 className="text-lg font-bold">{title}</h4>
                    </div>
                  </div>
                  {avg != null && (
                    <span className="flex items-center gap-1 rounded-full bg-white/20 px-3 py-1 text-sm font-semibold">
                      <Award className="h-4 w-4" />
                      {avg.toFixed(0)}%
                    </span>
                  )}
                </div>
              </div>
              <CardBody>
                {rows.length === 0 ? (
                  <p className="text-center text-sm text-slate-500 py-4">No results published yet.</p>
                ) : (
                  <ul className="space-y-3">
                    {rows.map((r) => (
                      <li
                        key={r.id}
                        className="flex items-center justify-between rounded-lg border border-slate-100 bg-slate-50/50 px-3 py-2.5"
                      >
                        <span className="font-medium text-slate-800">{r.subjectName}</span>
                        <span className="text-right">
                          <span className="block text-sm font-semibold text-slate-900">
                            {r.marks} / {r.maxMarks}
                          </span>
                          <span className={`text-xs font-bold ${theme.accent}`}>{r.grade}</span>
                        </span>
                      </li>
                    ))}
                  </ul>
                )}
              </CardBody>
            </Card>
          );
        })}
      </div>
    </>
  );
}
