import { useEffect, useState, type ChangeEvent, type FormEvent } from 'react';
import { Edit, Eye, Link2, Plus, Search, Trash2, User } from 'lucide-react';
import { msmsApi } from '../../api/msmsApi';
import { getErrorMessage } from '../../api/client';
import type { ParentDetails, ParentItem, StudentItem } from '../../types';
import { matchesSearch } from '../../utils/search';
import {
  Alert, Button, Card, CardBody, CardHeader, EmptyState, Input, Modal,
  PageHeader, Select, TableWrap, Td, Th,
} from '../../components/ui';

const emptyParent = {
  email: '', password: '', fullName: '', firstName: '', lastName: '', phone: '', photoUrl: '', studentId: '',
};

export function ParentsPage() {
  const [parents, setParents] = useState<ParentItem[]>([]);
  const [students, setStudents] = useState<StudentItem[]>([]);
  const [open, setOpen] = useState(false);
  const [linkOpen, setLinkOpen] = useState(false);
  const [profile, setProfile] = useState<ParentDetails | null>(null);
  const [editing, setEditing] = useState<ParentItem | null>(null);
  const [search, setSearch] = useState('');
  const [form, setForm] = useState(emptyParent);
  const [linkForm, setLinkForm] = useState({ parentId: '', studentId: '' });
  const [photoFile, setPhotoFile] = useState<File | null>(null);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const load = () => {
    msmsApi.people.listParents().then((r) => setParents(r.data));
    msmsApi.people.listAllStudents().then((r) => setStudents(r.data.items));
  };

  useEffect(() => { load(); }, []);

  const openCreate = () => {
    setEditing(null);
    setForm(emptyParent);
    setPhotoFile(null);
    setError('');
    setOpen(true);
  };

  const openEdit = (parent: ParentItem) => {
    setEditing(parent);
    setForm({
      email: parent.email ?? '',
      password: '',
      fullName: `${parent.firstName} ${parent.lastName}`,
      firstName: parent.firstName,
      lastName: parent.lastName,
      phone: parent.phone ?? '',
      photoUrl: parent.photoUrl ?? '',
      studentId: '',
    });
    setPhotoFile(null);
    setError('');
    setOpen(true);
  };

  const onPhotoChange = (e: ChangeEvent<HTMLInputElement>) => {
    setPhotoFile(e.target.files?.[0] ?? null);
  };

  const openProfile = async (id: string) => {
    setError('');
    try {
      const res = await msmsApi.people.getParent(id);
      setProfile(res.data);
    } catch (err) {
      setError(getErrorMessage(err));
    }
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
        await msmsApi.people.updateParent(editing.id, {
          firstName: form.firstName,
          lastName: form.lastName,
          phone: form.phone || null,
          photoUrl,
        });
      } else {
        await msmsApi.people.createParent({
          ...form,
          phone: form.phone || null,
          photoUrl,
          studentIds: form.studentId ? [form.studentId] : [],
        });
      }

      setOpen(false);
      setForm(emptyParent);
      load();
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  const linkChild = async (e: FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError('');
    try {
      await msmsApi.people.linkParentChild(linkForm.parentId, linkForm.studentId);
      setLinkOpen(false);
      setLinkForm({ parentId: '', studentId: '' });
      load();
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  const remove = async (parent: ParentItem) => {
    if (!confirm(`Delete ${parent.firstName} ${parent.lastName}?`)) return;
    setError('');
    try {
      await msmsApi.people.deleteParent(parent.id);
      load();
    } catch (err) {
      setError(getErrorMessage(err));
    }
  };
  const filteredParents = parents.filter((p) =>
    matchesSearch(search, [
      p.firstName,
      p.lastName,
      `${p.firstName} ${p.lastName}`,
      p.email,
      p.phone,
      p.linkedStudentCount,
    ])
  );

  return (
    <>
      <PageHeader
        title="Parents"
        description="Parent profiles, optional photos, child links, edit and delete"
        action={
          <span className="flex gap-2">
            <Button variant="secondary" onClick={() => setLinkOpen(true)}>
              <Link2 className="h-4 w-4" /> Link Child
            </Button>
            <Button onClick={openCreate}>
              <Plus className="h-4 w-4" /> Add Parent
            </Button>
          </span>
        }
      />
      {error && <div className="mb-4"><Alert message={error} /></div>}

      <Card>
        <CardHeader
          title="All Parents"
          subtitle={`${filteredParents.length} of ${parents.length} records`}
          action={
            <label className="relative block">
              <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
              <input
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                placeholder="Search name, phone, email..."
                className="w-64 rounded-xl border border-slate-200 bg-slate-50/80 py-2.5 pl-9 pr-3 text-sm outline-none transition focus:border-indigo-500 focus:bg-white focus:ring-4 focus:ring-indigo-500/10"
              />
            </label>
          }
        />
        <CardBody className="p-0">
          {filteredParents.length === 0 ? (
            <EmptyState message="No parents yet." />
          ) : (
            <TableWrap>
              <thead><tr><Th>Photo</Th><Th>Name</Th><Th>Email</Th><Th>Phone</Th><Th>Children</Th><Th>Actions</Th></tr></thead>
              <tbody>
                {filteredParents.map((p) => (
                  <tr key={p.id}>
                    <Td>{p.photoUrl ? <img src={p.photoUrl} alt="" className="h-10 w-10 rounded-full object-cover" /> : <User className="h-6 w-6 text-slate-300" />}</Td>
                    <Td className="font-medium">{p.firstName} {p.lastName}</Td>
                    <Td>{p.email ?? '—'}</Td>
                    <Td>{p.phone ?? '—'}</Td>
                    <Td>{p.linkedStudentCount}</Td>
                    <Td>
                      <div className="flex gap-2">
                        <Button size="sm" variant="secondary" onClick={() => openProfile(p.id)}><Eye className="h-4 w-4" /></Button>
                        <Button size="sm" variant="secondary" onClick={() => openEdit(p)}><Edit className="h-4 w-4" /></Button>
                        <Button size="sm" variant="danger" onClick={() => remove(p)}><Trash2 className="h-4 w-4" /></Button>
                      </div>
                    </Td>
                  </tr>
                ))}
              </tbody>
            </TableWrap>
          )}
        </CardBody>
      </Card>

      <Modal open={open} onClose={() => setOpen(false)} title={editing ? 'Edit Parent' : 'Add Parent'} wide>
        {error && <Alert message={error} />}
        <form onSubmit={submit} className="grid gap-3 sm:grid-cols-2">
          {!editing && (
            <>
              <Input label="Email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} required />
              <Input label="Password" type="password" value={form.password} onChange={(e) => setForm({ ...form, password: e.target.value })} required />
              <Input label="Full Name" className="sm:col-span-2" value={form.fullName} onChange={(e) => setForm({ ...form, fullName: e.target.value })} required />
            </>
          )}
          <Input label="First Name" value={form.firstName} onChange={(e) => setForm({ ...form, firstName: e.target.value })} required />
          <Input label="Last Name" value={form.lastName} onChange={(e) => setForm({ ...form, lastName: e.target.value })} required />
          <Input label="Phone" value={form.phone} onChange={(e) => setForm({ ...form, phone: e.target.value })} />
          <Input label="Photo URL (optional)" value={form.photoUrl} onChange={(e) => setForm({ ...form, photoUrl: e.target.value })} />
          <label className="block">
            <span className="mb-2 block text-sm font-medium text-slate-700">Upload photo (optional)</span>
            <input type="file" accept="image/*" onChange={onPhotoChange} className="w-full rounded-xl border border-slate-200 px-4 py-2.5 text-sm" />
          </label>
          {!editing && (
            <Select label="Link student now (optional)" value={form.studentId} onChange={(e) => setForm({ ...form, studentId: e.target.value })}>
              <option value="">— No child yet —</option>
              {students.map((s) => (
                <option key={s.id} value={s.id}>{s.firstName} {s.lastName} ({s.studentNumber})</option>
              ))}
            </Select>
          )}
          <div className="sm:col-span-2 flex justify-end gap-2 pt-2">
            <Button type="button" variant="secondary" onClick={() => setOpen(false)}>Cancel</Button>
            <Button type="submit" loading={loading}>{editing ? 'Update' : 'Save'}</Button>
          </div>
        </form>
      </Modal>

      <Modal open={linkOpen} onClose={() => setLinkOpen(false)} title="Link Parent to Student">
        {error && <Alert message={error} />}
        <form onSubmit={linkChild} className="space-y-3">
          <Select label="Parent" value={linkForm.parentId} onChange={(e) => setLinkForm({ ...linkForm, parentId: e.target.value })} required>
            <option value="">— Select parent —</option>
            {parents.map((p) => <option key={p.id} value={p.id}>{p.firstName} {p.lastName}</option>)}
          </Select>
          <Select label="Student" value={linkForm.studentId} onChange={(e) => setLinkForm({ ...linkForm, studentId: e.target.value })} required>
            <option value="">— Select student —</option>
            {students.map((s) => <option key={s.id} value={s.id}>{s.firstName} {s.lastName} ({s.studentNumber})</option>)}
          </Select>
          <div className="flex justify-end gap-2 pt-2">
            <Button type="button" variant="secondary" onClick={() => setLinkOpen(false)}>Cancel</Button>
            <Button type="submit" loading={loading}>Link</Button>
          </div>
        </form>
      </Modal>

      <Modal open={!!profile} onClose={() => setProfile(null)} title="Parent Profile" wide>
        {profile && (
          <div className="space-y-5">
            <div className="flex items-center gap-4 rounded-2xl bg-slate-50 p-4">
              {profile.photoUrl ? <img src={profile.photoUrl} alt="" className="h-20 w-20 rounded-2xl object-cover" /> : <User className="h-14 w-14 text-slate-300" />}
              <div>
                <h3 className="text-xl font-bold text-slate-900">{profile.firstName} {profile.lastName}</h3>
                <p className="text-sm text-slate-500">{profile.email ?? 'No email'} · {profile.phone ?? 'No phone'}</p>
              </div>
            </div>
            <div>
              <h4 className="mb-2 font-semibold text-slate-900">Linked students</h4>
              {profile.children.length === 0 ? <EmptyState message="No linked students." /> : (
                <ul className="grid gap-2 sm:grid-cols-2">
                  {profile.children.map((s) => (
                    <li key={s.id} className="rounded-xl border border-slate-100 px-4 py-3 text-sm">
                      <span className="font-semibold">{s.firstName} {s.lastName}</span>
                      <span className="block text-slate-500">{s.studentNumber} · {s.className ?? 'No class'}</span>
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
