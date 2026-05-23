import { useEffect, useState } from 'react';
import { BookOpen, Award, TrendingUp } from 'lucide-react';
import { Link } from 'react-router-dom';
import { msmsApi } from '../../api/msmsApi';
import { Card, CardBody, CardHeader, PageHeader, StatCard } from '../../components/ui';

export function StudentDashboard() {
  const [count, setCount] = useState(0);

  useEffect(() => {
    msmsApi.student.myResults().then((r) => setCount(r.data.length));
  }, []);

  return (
    <>
      <PageHeader
        badge="Student"
        title="My Dashboard"
        description="View your exam results and academic performance."
      />
      <section className="mb-8 grid gap-5 sm:grid-cols-3">
        <StatCard label="Exam results" value={count} icon={BookOpen} theme="violet" />
        <StatCard label="Status" value="Active" icon={Award} theme="emerald" />
        <StatCard label="Progress" value="On track" icon={TrendingUp} theme="sky" />
      </section>
      <Card>
        <CardHeader title="Academics" subtitle="Your published results" />
        <CardBody>
          <Link
            to="/student/results"
            className="inline-flex items-center gap-2 rounded-xl bg-gradient-to-r from-indigo-600 to-violet-600 px-5 py-3 text-sm font-semibold text-white shadow-md shadow-indigo-500/25 transition hover:from-indigo-700"
          >
            <BookOpen className="h-4 w-4" />
            View all results
          </Link>
        </CardBody>
      </Card>
    </>
  );
}
