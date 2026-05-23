import { useEffect, useState, type ChangeEvent, type FormEvent } from 'react';
import { Edit, Eye, Plus, Search, Trash2, User } from 'lucide-react';
import { msmsApi } from '../../api/msmsApi';
import { getErrorMessage } from '../../api/client';
import type { TeacherDetails, TeacherItem } from '../../types';
import { matchesSearch } from '../../utils/search';
import {
  Alert, Button, Card, CardBody, CardHeader, EmptyState, Input, Modal,
  PageHeader, TableWrap, Td, Th,
} from '../../components/ui';

const emptyForm = {
  email: '', password: '', fullName: '', employeeNumber: '', firstName: '', lastName: '', photoUrl: '',
};

export function TeachersPage() {
  const [teachers, setTeachers] = useState<TeacherItem[]>([]);
  const [open, setOpen] = useState(false);
  const [profile, setProfile] = useState<TeacherDetails | null>(null);
  const [editing, setEditing] = useState<TeacherItem | null>(null);
  const [search, setSearch] = useState('');
  const [form, setForm] = useState(emptyForm);
  const [photoFile, setPhotoFile] = useState<File | null>(null);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const load = () => msmsApi.people.listTeachers().then((r) => setTeachers(r.data));

  useEffect(() => { load(); }, []);

  const openCreate = () => {
    setEditing(null);
    setForm(emptyForm);
    setPhotoFile(null);
    setError('');
    setOpen(true);
  };

  const openEdit = (teacher: TeacherItem) => {
    setEditing(teacher);
    setForm({
      email: teacher.email ?? '',
      password: '',
      fullName: `${teacher.firstName} ${teacher.lastName}`,
      employeeNumber: teacher.employeeNumber,
      firstName: teacher.firstName,
      lastName: teacher.lastName,
      photoUrl: teacher.photoUrl ?? '',
    });
    setPhotoFile(null);
    setError('');
    setOpen(true);
  };

  const openProfile = async (id: string) => {
    setError('');
    try {
      const res = await msmsApi.people.getTeacher(id);
      setProfile(res.data);
    } catch (err) {
      setError(getErrorMessage(err));
    }
  };

  const onPhotoChange = (e: ChangeEvent<HTMLInputElement>) => {
    setPhotoFile(e.target.files?.[0] ?? null);
  };

  const submit = async (e: FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError('');
    try {
      let photoUrl = form.photoUrl || null;
      if (photoFile) {
        const up = await msmsApi.people.uploadPhoto(photoFile);
        photoUrl = up.data.url;
      }

      if (editing) {
        await msmsApi.people.updateTeacher(editing.id, {
          employeeNumber: form.employeeNumber,
          firstName: form.firstName,
          lastName: form.lastName,
          photoUrl,
        });
      } else {
        await msmsApi.people.createTeacher({ ...form, photoUrl });
      }

      setOpen(false);
      load();
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  const remove = async (teacher: TeacherItem) => {
    if (!confirm(`Delete ${teacher.firstName} ${teacher.lastName}?`)) return;
    setError('');
    try {
      await msmsApi.people.deleteTeacher(teacher.id);
      load();
    } catch (err) {
      setError(getErrorMessage(err));
    }
  };
  const filteredTeachers = teachers.filter((t) =>
    matchesSearch(search, [
      t.employeeNumber,
      t.firstName,
      t.lastName,
      `${t.firstName} ${t.lastName}`,
      t.email,
      t.assignmentCount,
    ])
  );

  return (
    <>
      <PageHeader
        title="Teachers"
        description="Manage teaching staff, photos, profiles, assignments, edit and delete"
        action={<Button onClick={openCreate}><Plus className="h-4 w-4" /> Add Teacher</Button>}
      />
      {error && <div className="mb-4"><Alert message={error} /></div>}

      <Card>
        <CardHeader
          title="All Teachers"
          subtitle={`${filteredTeachers.length} of ${teachers.length} records`}
          action={
            <label className="relative block">
              <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
              <input
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                placeholder="Search name, employee #, email..."
                className="w-64 rounded-xl border border-slate-200 bg-slate-50/80 py-2.5 pl-9 pr-3 text-sm outline-none transition focus:border-indigo-500 focus:bg-white focus:ring-4 focus:ring-indigo-500/10"
              />
            </label>
          }
        />
        <CardBody className="p-0">
          {filteredTeachers.length === 0 ? (
            <EmptyState message="No teachers yet." />
          ) : (
            <TableWrap>
              <thead><tr><Th>Photo</Th><Th>Employee #</Th><Th>Name</Th><Th>Email</Th><Th>Subjects</Th><Th>Actions</Th></tr></thead>
              <tbody>
                {filteredTeachers.map((t) => (
                  <tr key={t.id}>
                    <Td>
                      {t.photoUrl ? <img src={t.photoUrl} alt="" className="h-10 w-10 rounded-full object-cover" /> : <User className="h-6 w-6 text-slate-300" />}
                    </Td>
                    <Td>{t.employeeNumber}</Td>
                    <Td className="font-medium">{t.firstName} {t.lastName}</Td>
                    <Td>{t.email ?? '—'}</Td>
                    <Td>{t.assignmentCount}</Td>
                    <Td>
                      <div className="flex gap-2">
                        <Button size="sm" variant="secondary" onClick={() => openProfile(t.id)}><Eye className="h-4 w-4" /></Button>
                        <Button size="sm" variant="secondary" onClick={() => openEdit(t)}><Edit className="h-4 w-4" /></Button>
                        <Button size="sm" variant="danger" onClick={() => remove(t)}><Trash2 className="h-4 w-4" /></Button>
                      </div>
                    </Td>
                  </tr>
                ))}
              </tbody>
            </TableWrap>
          )}
        </CardBody>
      </Card>

      <Modal open={open} onClose={() => setOpen(false)} title={editing ? 'Edit Teacher' : 'Add Teacher'} wide>
        {error && <Alert message={error} />}
        <form onSubmit={submit} className="grid gap-3 sm:grid-cols-2">
          {!editing && (
            <>
              <Input label="Email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} required />
              <Input label="Password" type="password" value={form.password} onChange={(e) => setForm({ ...form, password: e.target.value })} required />
              <Input label="Full Name" className="sm:col-span-2" value={form.fullName} onChange={(e) => setForm({ ...form, fullName: e.target.value })} required />
            </>
          )}
          <Input label="Employee Number" value={form.employeeNumber} onChange={(e) => setForm({ ...form, employeeNumber: e.target.value })} required />
          <Input label="First Name" value={form.firstName} onChange={(e) => setForm({ ...form, firstName: e.target.value })} required />
          <Input label="Last Name" value={form.lastName} onChange={(e) => setForm({ ...form, lastName: e.target.value })} required />
          <Input label="Photo URL (optional)" value={form.photoUrl} onChange={(e) => setForm({ ...form, photoUrl: e.target.value })} />
          <label className="block">
            <span className="mb-2 block text-sm font-medium text-slate-700">Upload photo (optional)</span>
            <input type="file" accept="image/*" onChange={onPhotoChange} className="w-full rounded-xl border border-slate-200 px-4 py-2.5 text-sm" />
          </label>
          <div className="sm:col-span-2 flex justify-end gap-2 pt-2">
            <Button type="button" variant="secondary" onClick={() => setOpen(false)}>Cancel</Button>
            <Button type="submit" loading={loading}>{editing ? 'Update' : 'Save'}</Button>
          </div>
        </form>
      </Modal>

      <Modal open={!!profile} onClose={() => setProfile(null)} title="Teacher Profile" wide>
        {profile && (
          <div className="space-y-5">
            <div className="flex items-center gap-4 rounded-2xl bg-slate-50 p-4">
              {profile.photoUrl ? <img src={profile.photoUrl} alt="" className="h-20 w-20 rounded-2xl object-cover" /> : <User className="h-14 w-14 text-slate-300" />}
              <div>
                <h3 className="text-xl font-bold text-slate-900">{profile.firstName} {profile.lastName}</h3>
                <p className="text-sm text-slate-500">{profile.employeeNumber} · {profile.email ?? 'No email'}</p>
              </div>
            </div>
            <div>
              <h4 className="mb-2 font-semibold text-slate-900">Assigned classes and subjects</h4>
              {profile.assignments.length === 0 ? <EmptyState message="No assignments." /> : (
                <ul className="grid gap-2 sm:grid-cols-2">
                  {profile.assignments.map((a) => (
                    <li key={`${a.classId}-${a.subjectId}`} className="rounded-xl border border-slate-100 px-4 py-3 text-sm">
                      <span className="font-semibold">{a.subjectName}</span>
                      <span className="block text-slate-500">{a.className}</span>
                    </li>
                  ))}
                </ul>
              )}
            </div>
          </div>
        )}
      </Modal>
    </>
  );
}
