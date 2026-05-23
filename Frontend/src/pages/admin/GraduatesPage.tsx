import { useEffect, useState, type FormEvent } from 'react';
import { GraduationCap, Sparkles } from 'lucide-react';
import { msmsApi } from '../../api/msmsApi';
import { getErrorMessage } from '../../api/client';
import type { ClassItem, GraduatedStudent, StudentItem } from '../../types';
import {
  Alert, Button, Card, CardBody, CardHeader, EmptyState, Input, PageHeader, Select, TableWrap, Td, Th,
} from '../../components/ui';

export function GraduatesPage() {
  const [graduated, setGraduated] = useState<GraduatedStudent[]>([]);
  const [students, setStudents] = useState<StudentItem[]>([]);
  const [classes, setClasses] = useState<ClassItem[]>([]);
  const [error, setError] = useState('');
  const [message, setMessage] = useState('');
  const [loading, setLoading] = useState(false);

  const [promoteForm, setPromoteForm] = useState({
    studentId: '', toClassId: '', promotionDate: new Date().toISOString().slice(0, 10), academicYear: '2025-2026',
  });
  const [graduateForm, setGraduateForm] = useState({
    studentId: '', graduationDate: new Date().toISOString().slice(0, 10), academicYear: '2025-2026',
  });
  const [autoForm, setAutoForm] = useState({
    newAcademicYear: '2026-2027',
    promotionDate: new Date().toISOString().slice(0, 10),
  });

  const load = () => {
    msmsApi.lifecycle.listGraduated().then((r) => setGraduated(r.data));
    msmsApi.people.listAllStudents().then((r) => setStudents(r.data.items));
    msmsApi.school.listClasses().then((r) => setClasses(r.data));
  };

  useEffect(() => { load(); }, []);

  const promote = async (e: FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError('');
    try {
      await msmsApi.lifecycle.promote(promoteForm);
      setMessage('Student promoted.');
      load();
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  const autoPromote = async (e: FormEvent) => {
    e.preventDefault();
    if (!confirm('Promote all students to the next grade? Grade 12 students will graduate automatically.')) return;
    setLoading(true);
    setError('');
    try {
      const res = await msmsApi.lifecycle.autoPromote(autoForm);
      setMessage(
        `Done: ${res.data.promotedCount} promoted, ${res.data.graduatedCount} graduated.`
      );
      load();
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  const graduate = async (e: FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError('');
    try {
      await msmsApi.lifecycle.graduate(graduateForm);
      setMessage('Student graduated.');
      load();
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <PageHeader title="Graduates & Promotions" description="Promote students or record graduation" />
      {error && <span className="mb-4 block"><Alert message={error} /></span>}
      {message && <span className="mb-4 block"><Alert message={message} type="success" /></span>}

      <Card className="mb-6 border-indigo-200 bg-gradient-to-br from-indigo-50/80 to-white">
        <CardHeader
          title="End of year — Auto promote all"
          subtitle="Primary 1–4, Middle 5–8, High 9–12. Grade 7→8, grade 12→graduated automatically."
        />
        <CardBody>
          <form onSubmit={autoPromote} className="flex flex-col gap-3 sm:flex-row sm:items-end">
            <Input
              label="New academic year"
              value={autoForm.newAcademicYear}
              onChange={(e) => setAutoForm({ ...autoForm, newAcademicYear: e.target.value })}
              className="flex-1"
            />
            <Input
              label="Promotion date"
              type="date"
              value={autoForm.promotionDate}
              onChange={(e) => setAutoForm({ ...autoForm, promotionDate: e.target.value })}
              className="flex-1"
            />
            <Button type="submit" loading={loading}>
              <Sparkles className="h-4 w-4" /> Auto promote all students
            </Button>
          </form>
        </CardBody>
      </Card>

      <section className="mb-6 grid gap-6 lg:grid-cols-2">
        <Card>
          <CardHeader title="Promote Student" />
          <CardBody>
            <form onSubmit={promote} className="space-y-3">
              <Select label="Student" value={promoteForm.studentId} onChange={(e) => setPromoteForm({ ...promoteForm, studentId: e.target.value })} required>
                <option value="">— Select —</option>
                {students.map((s) => (
                  <option key={s.id} value={s.id}>{s.firstName} {s.lastName}</option>
                ))}
              </Select>
              <Select label="To Class" value={promoteForm.toClassId} onChange={(e) => setPromoteForm({ ...promoteForm, toClassId: e.target.value })} required>
                <option value="">— Select —</option>
                {classes.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
              </Select>
              <Input label="Date" type="date" value={promoteForm.promotionDate} onChange={(e) => setPromoteForm({ ...promoteForm, promotionDate: e.target.value })} />
              <Input label="Academic Year" value={promoteForm.academicYear} onChange={(e) => setPromoteForm({ ...promoteForm, academicYear: e.target.value })} />
              <Button type="submit" loading={loading}>Promote</Button>
            </form>
          </CardBody>
        </Card>

        <Card>
          <CardHeader title="Graduate Student" />
          <CardBody>
            <form onSubmit={graduate} className="space-y-3">
              <Select label="Student" value={graduateForm.studentId} onChange={(e) => setGraduateForm({ ...graduateForm, studentId: e.target.value })} required>
                <option value="">— Select —</option>
                {students.map((s) => (
                  <option key={s.id} value={s.id}>{s.firstName} {s.lastName}</option>
                ))}
              </Select>
              <Input label="Graduation Date" type="date" value={graduateForm.graduationDate} onChange={(e) => setGraduateForm({ ...graduateForm, graduationDate: e.target.value })} />
              <Input label="Academic Year" value={graduateForm.academicYear} onChange={(e) => setGraduateForm({ ...graduateForm, academicYear: e.target.value })} />
              <Button type="submit" loading={loading}><GraduationCap className="h-4 w-4" /> Graduate</Button>
            </form>
          </CardBody>
        </Card>
      </section>

      <Card>
        <CardHeader title="Graduated Students" subtitle={`${graduated.length} archived`} />
        <CardBody className="p-0">
          {graduated.length === 0 ? (
            <EmptyState message="No graduates yet." />
          ) : (
            <TableWrap>
              <thead><tr><Th>Number</Th><Th>Name</Th><Th>Date</Th><Th>Year</Th></tr></thead>
              <tbody>
                {graduated.map((g) => (
                  <tr key={g.id}>
                    <Td>{g.studentNumber}</Td>
                    <Td className="font-medium">{g.fullName}</Td>
                    <Td>{g.graduationDate}</Td>
                    <Td>{g.academicYear}</Td>
                  </tr>
                ))}
              </tbody>
            </TableWrap>
          )}
        </CardBody>
      </Card>
    </>
  );
}
